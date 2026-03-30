using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace GroundTruthCuration.Infrastructure.Utilities;

/// <summary>
/// Helper class for creating SQL connections with Azure AD token authentication.
/// Uses a ChainedTokenCredential prioritizing Azure CLI for local development.
/// </summary>
public static class SqlConnectionHelper
{
    private static readonly TokenRequestContext SqlTokenRequest = new(new[] { "https://database.windows.net/.default" });

    private static readonly ChainedTokenCredential Credential = new ChainedTokenCredential(
        new AzureCliCredential(),
        new ManagedIdentityCredential()
    );

    /// <summary>
    /// Creates and opens a SQL connection using Azure AD token authentication.
    /// This method keeps "Authentication=Active Directory Interactive" if present,  
    /// or explicitly acquires an access token using ChainedTokenCredential.
    /// </summary>
    /// <param name="connectionString">The base connection string.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An open SqlConnection with token-based authentication.</returns>
    /// <exception cref="InvalidOperationException">Thrown when token acquisition fails.</exception>
    public static async Task<SqlConnection> CreateAndOpenConnectionAsync(
        string connectionString,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        // Check if connection string already has authentication method set
        var builder = new SqlConnectionStringBuilder(connectionString);

        // If using Active Directory Interactive, let SQL Client handle it (works like VS Code)
        if (builder.Authentication == SqlAuthenticationMethod.ActiveDirectoryInteractive)
        {
            logger?.LogInformation("Using Active Directory Interactive authentication (same as VS Code SQL extension)");
            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellationToken);

                logger?.LogInformation("SQL connection opened successfully to {Server}/{Database}",
                    builder.DataSource,
                    builder.InitialCatalog);

                return connection;
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 47073)
            {
                logger?.LogError(sqlEx, "Failed to open SQL connection with Active Directory Interactive authentication (error 47073)");
                throw new InvalidOperationException(
                    "Failed to connect to SQL Server using Active Directory Interactive authentication. " +
                    "Ensure you have the necessary networking permissions to allow access to the SQL Server for local development. " +
                    $"Error: {sqlEx.Message}",
                    sqlEx);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to open SQL connection with Active Directory Interactive authentication");
                throw new InvalidOperationException(
                    "Failed to connect to SQL Server using Active Directory Interactive authentication. " +
                    "Ensure you have the necessary tools installed and configured for interactive auth. " +
                    $"Error: {ex.Message}",
                    ex);
            }
        }

        // Otherwise, use token-based authentication
        builder.Authentication = SqlAuthenticationMethod.NotSpecified;

        var cleanConnectionString = builder.ConnectionString;

        logger?.LogInformation("Acquiring Azure AD access token for SQL Database using ChainedTokenCredential (AzureCliCredential -> ManagedIdentityCredential)");

        try
        {
            // Acquire access token using ChainedTokenCredential
            var tokenResult = await Credential.GetTokenAsync(SqlTokenRequest, cancellationToken);

            logger?.LogInformation("Successfully acquired Azure AD access token (expires: {Expiry})", tokenResult.ExpiresOn);

            // Decode token to log the identity being used
            if (logger != null)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(tokenResult.Token);
                    var upn = jwtToken.Claims.FirstOrDefault(c => c.Type == "upn")?.Value;
                    var oid = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
                    var appId = jwtToken.Claims.FirstOrDefault(c => c.Type == "appid")?.Value;

                    logger.LogWarning("🔐 CONNECTING TO SQL AS: UPN={UPN}, OID={OID}, AppId={AppId}",
                        upn ?? "N/A", oid ?? "N/A", appId ?? "N/A");
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Failed to parse token claims: {Error}", ex.Message);
                }
            }

            var connection = new SqlConnection(cleanConnectionString)
            {
                AccessToken = tokenResult.Token
            };

            await connection.OpenAsync(cancellationToken);

            logger?.LogDebug("SQL connection opened successfully to {Server}/{Database}",
                builder.DataSource,
                builder.InitialCatalog);

            return connection;
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 4060)
        {
            // Database does not exist or cannot be accessed
            logger?.LogError(sqlEx,
                "❌ DATABASE NOT FOUND: Cannot open database '{Database}' on server '{Server}'. " +
                "Check your connection string in appsettings.development.json. " +
                "Common issues: wrong database name, database not created yet, or no access permissions.",
                builder.InitialCatalog,
                builder.DataSource);

            throw new InvalidOperationException(
                $"Cannot connect to database '{builder.InitialCatalog}' on server '{builder.DataSource}'. " +
                $"Verify the database name in your connection string (appsettings.development.json). " +
                $"SQL Error {sqlEx.Number}: {sqlEx.Message}",
                sqlEx);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 18456)
        {
            // Login failed - authentication issue
            logger?.LogError(sqlEx,
                "❌ AUTHENTICATION FAILED: Login failed for user. " +
                "Ensure you are logged in via 'az login' and your user exists in the database. " +
                "Run this SQL as admin: CREATE USER [your-email@domain.com] FROM EXTERNAL PROVIDER;");

            throw new InvalidOperationException(
                "SQL Server authentication failed. Ensure you are logged in via 'az login' and " +
                "your Azure AD user has been created in the database with appropriate roles. " +
                $"SQL Error {sqlEx.Number}: {sqlEx.Message}",
                sqlEx);
        }
        catch (SqlException sqlEx)
        {
            // Other SQL errors
            logger?.LogError(sqlEx,
                "❌ SQL CONNECTION ERROR: Server='{Server}', Database='{Database}', Error={ErrorNumber}",
                builder.DataSource,
                builder.InitialCatalog,
                sqlEx.Number);

            throw new InvalidOperationException(
                $"Failed to connect to SQL Server '{builder.DataSource}', Database '{builder.InitialCatalog}'. " +
                $"SQL Error {sqlEx.Number}: {sqlEx.Message}",
                sqlEx);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to create SQL connection with Azure AD authentication");
            throw new InvalidOperationException(
                "Failed to acquire Azure AD access token for SQL Database. " +
                "Ensure you are logged in via 'az login' or have appropriate managed identity configured. " +
                $"Error: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Synchronous version of CreateAndOpenConnectionAsync.
    /// </summary>
    public static SqlConnection CreateAndOpenConnection(
        string connectionString,
        ILogger? logger = null)
    {
        return CreateAndOpenConnectionAsync(connectionString, logger).GetAwaiter().GetResult();
    }
}
