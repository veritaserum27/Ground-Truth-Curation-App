#!/usr/bin/env python
"""Shared helpers for Ground Truth SQL database seeding utilities."""

from __future__ import annotations

import logging
import os
import re
import struct
from pathlib import Path
from typing import Any, Iterable, List

import pyodbc

try:
    from dotenv import load_dotenv  # type: ignore[import]
except ImportError as exc:  # pragma: no cover - dependency load
    raise SystemExit(
        "The 'python-dotenv' package is required. Install it via "
        "'pip install python-dotenv'."
    ) from exc

LOGGER = logging.getLogger("ground_truth_sql")
DEFAULT_DRIVER = "{ODBC Driver 18 for SQL Server}"
TOKEN_SCOPE = "https://database.windows.net/.default"
SQL_COPT_SS_ACCESS_TOKEN = getattr(pyodbc, "SQL_COPT_SS_ACCESS_TOKEN", 1256)
CONNECTION_ENV_VAR = "GT_AZURE_SQL_CONNECTIONSTRING"
DEFAULT_TIMEOUT_SECONDS = 60


def load_environment(base_dir: Path | None) -> None:
    """Load environment variables from a .env file if present.

    Looks for .env in the following order:
    1. base_dir/.env (if base_dir provided)
    2. Parent directory (infra/seed/.env)
    3. Current working directory
    """
    if base_dir is not None:
        env_path = base_dir / ".env"
        if env_path.exists():
            load_dotenv(dotenv_path=env_path)
            LOGGER.debug("Loaded environment variables from %s", env_path)
            return

    # Try parent directory (for new structure: groundtruth/ -> infra/seed/)
    parent_env = Path(__file__).resolve().parent.parent / ".env"
    if parent_env.exists():
        load_dotenv(dotenv_path=parent_env)
        LOGGER.debug("Loaded environment variables from %s", parent_env)
        return

    load_dotenv()
    LOGGER.debug("Loaded environment variables using default search path")


def resolve_connection_string(override: str | None) -> str:
    """Resolve the Azure SQL ODBC connection string."""
    candidate = override.strip() if override else ""
    if not candidate:
        env_value = os.getenv(CONNECTION_ENV_VAR, "").strip()
        if not env_value:
            raise ValueError(
                "Connection string not provided. Supply --connection-string "
                f"or set {CONNECTION_ENV_VAR} as described in the Azure SQL "
                "Python quickstart."
            )
        candidate = env_value
        LOGGER.debug(
            "Using connection string from %s environment variable.",
            CONNECTION_ENV_VAR,
        )
    else:
        LOGGER.debug(
            "Using connection string provided via --connection-string "
            "argument."
        )

    if "driver=" not in candidate.lower():
        LOGGER.debug(
            "Driver segment missing; prefixing connection string with %s.",
            DEFAULT_DRIVER,
        )
        candidate = f"Driver={DEFAULT_DRIVER};{candidate}"

    return candidate


def acquire_access_token() -> bytes:
    """Acquire an Azure AD access token for Azure SQL Database."""
    try:
        from azure.core.exceptions import ClientAuthenticationError
        from azure.identity import DefaultAzureCredential
    except ImportError as exc:  # pragma: no cover - dependency load
        raise SystemExit(
            "The 'azure-identity' package is required. Install it via "
            "'pip install azure-identity'."
        ) from exc

    credential = DefaultAzureCredential(
        exclude_interactive_browser_credential=False
    )
    try:
        token = credential.get_token(TOKEN_SCOPE)
    except ClientAuthenticationError as exc:  # pragma: no cover - auth failure
        raise ValueError(
            "Failed to obtain an Azure AD access token. Run 'az login' or "
            "configure credentials before executing this script."
        ) from exc

    token_bytes = token.token.encode("utf-16-le")
    token_struct = struct.pack(
        f"<I{len(token_bytes)}s",
        len(token_bytes),
        token_bytes,
    )
    LOGGER.debug("Successfully acquired Azure AD access token.")
    return token_struct


def open_connection(
    connection_string: str,
    *,
    autocommit: bool = True,
    timeout: int = DEFAULT_TIMEOUT_SECONDS,
) -> pyodbc.Connection:
    """Open a pyodbc connection using passwordless Azure AD authentication."""
    token_struct = acquire_access_token()
    attrs_before = {SQL_COPT_SS_ACCESS_TOKEN: token_struct}
    LOGGER.debug("Opening pyodbc connection with token-based authentication.")
    return pyodbc.connect(
        connection_string,
        attrs_before=attrs_before,
        autocommit=autocommit,
        timeout=timeout,
    )


def load_sql_file(path: Path) -> List[str]:
    """Load a SQL file and split it into executable statements."""
    LOGGER.debug("Loading SQL file: %s", path)
    if not path.exists():
        raise FileNotFoundError(f"SQL file not found: {path}")

    content = path.read_text(encoding="utf-8")
    statements = [
        stmt.strip()
        for stmt in re.split(
            r"^\s*GO\s*$", content, flags=re.IGNORECASE | re.MULTILINE
        )
        if stmt.strip()
    ]
    LOGGER.debug("Loaded %s statements from %s", len(statements), path.name)
    return statements


def execute_statements(connection: Any, statements: Iterable[str]) -> None:
    """Execute a sequence of SQL statements using the provided connection."""
    cursor = connection.cursor()
    for index, statement in enumerate(statements, start=1):
        LOGGER.debug("Executing statement %s", index)
        cursor.execute(statement)
    cursor.commit()


def execute_sql_file(connection: Any, sql_path: Path) -> None:
    """Run all statements in the given SQL script file."""
    LOGGER.info("Running %s", sql_path.name)
    statements = load_sql_file(sql_path)
    try:
        execute_statements(connection, statements)
    except pyodbc.Error as exc:
        LOGGER.error("SQL execution failed for %s", sql_path.name)
        raise RuntimeError(f"Error executing {sql_path.name}") from exc


def run_sql_scripts(
    connection_string: str,
    scripts: Iterable[Path],
    *,
    autocommit: bool = True,
    timeout: int = DEFAULT_TIMEOUT_SECONDS,
) -> None:
    """Execute a list of SQL scripts against the target database."""
    connection = open_connection(
        connection_string,
        autocommit=autocommit,
        timeout=timeout,
    )
    try:
        for sql_path in scripts:
            execute_sql_file(connection, sql_path)
    finally:
        connection.close()
        LOGGER.debug("Closed SQL connection after executing scripts.")
