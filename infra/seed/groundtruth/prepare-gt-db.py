#!/usr/bin/env python
"""Ground Truth Database Preparation Utility.

Executes the core Ground Truth schema scripts against a target SQL Server
database.

Scripts executed (in order):
1. create-ground-truth-tables.sql
2. create-ground-truth-tag-relationships.sql

The tool reads SQL files from the directory containing this script by default.
It follows the Microsoft quickstart for Azure SQL with Python by retrieving
the ODBC connection string from the `GT_AZURE_SQL_CONNECTIONSTRING`
environment variable (or `--connection-string` argument) and using
passwordless authentication with Azure AD tokens.

Usage example:
    export GT_AZURE_SQL_CONNECTIONSTRING='Driver={ODBC Driver 18 for SQL Server};\
        Server=tcp:myserver.database.windows.net,1433;\
        Database=GroundTruthDB;Encrypt=yes;TrustServerCertificate=no;\
        Connection Timeout=30'
    az login
    python prepare-gt-db.py --verbose
    # Optionally seed sample data:
    python prepare-gt-db.py --seed

Exit codes:
    0 - Success
    1 - Configuration or argument error
    2 - Database connection error
    3 - SQL execution error
    99 - Unexpected error
"""

from __future__ import annotations

import argparse
import logging
import sys
from pathlib import Path
from typing import Iterable

from sql_utils import (  # type: ignore[import]
    load_environment,
    resolve_connection_string,
    run_sql_scripts,
)

LOGGER = logging.getLogger("prepare_gt_db")
SQL_SCRIPTS = (
    "01-create-ground-truth-tables.sql",
    "02-create-ground-truth-tag-relationships.sql",
)
SEED_SCRIPT_NAME = "04-insert-sample-ground-truth-data.sql"


def parse_arguments(argv: Iterable[str] | None = None) -> argparse.Namespace:
    """Parse command-line arguments."""
    parser = argparse.ArgumentParser(
        description=(
            "Execute mandatory Ground Truth schema scripts against a SQL "
            "Server database."
        )
    )

    parser.add_argument(
        "--verbose",
        "-v",
        action="store_true",
        help="Enable debug logging for troubleshooting.",
    )
    parser.add_argument(
        "--scripts-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "sql",
        help="Directory containing SQL scripts (default: sql/ subdirectory).",
    )
    parser.add_argument(
        "--connection-string",
        type=str,
        help=(
            "ODBC connection string for Azure SQL. When omitted, the script "
            "uses the GT_AZURE_SQL_CONNECTIONSTRING environment variable."
        ),
    )
    parser.add_argument(
        "--seed",
        action="store_true",
        help=(
            "Execute the sample data seed script after creating the schema."
        ),
    )
    parser.add_argument(
        "--seed-file",
        type=Path,
        help=(
            "Optional override path for the SQL seed script. Defaults to "
            f"{SEED_SCRIPT_NAME} within --scripts-dir."
        ),
    )

    return parser.parse_args(argv)


def configure_logging(verbose: bool) -> None:
    """Configure logging level and format."""
    level = logging.DEBUG if verbose else logging.INFO
    fmt = (
        "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
        if verbose
        else "%(levelname)s: %(message)s"
    )
    logging.basicConfig(
        level=level,
        format=fmt,
        datefmt="%Y-%m-%d %H:%M:%S",
    )


def build_schema_script_paths(base_dir: Path) -> list[Path]:
    """Resolve schema script paths and validate they exist."""
    resolved_base = base_dir.resolve()
    scripts: list[Path] = []
    for script_name in SQL_SCRIPTS:
        script_path = resolved_base / script_name
        if not script_path.exists():
            raise FileNotFoundError(f"SQL script not found: {script_path}")
        scripts.append(script_path)
    return scripts


def resolve_seed_script(base_dir: Path, override: Path | None) -> Path:
    """Determine which seed script to execute and ensure it exists."""
    if override is not None:
        candidate = override.resolve()
    else:
        candidate = base_dir.resolve() / SEED_SCRIPT_NAME

    if not candidate.exists():
        raise FileNotFoundError(f"Seed script not found: {candidate}")

    return candidate


def prepare_database(args: argparse.Namespace) -> None:
    """Execute schema (and optional seed) scripts using shared helpers."""
    load_environment(args.scripts_dir)
    connection_string = resolve_connection_string(args.connection_string)

    schema_scripts = build_schema_script_paths(args.scripts_dir)
    LOGGER.info("Executing Ground Truth schema scripts.")
    run_sql_scripts(connection_string, schema_scripts)

    if args.seed or args.seed_file is not None:
        seed_path = resolve_seed_script(args.scripts_dir, args.seed_file)
        LOGGER.info("Executing seed script: %s", seed_path.name)
        run_sql_scripts(connection_string, [seed_path])


def main(argv: Iterable[str] | None = None) -> int:
    """Entry point for command-line execution."""
    try:
        args = parse_arguments(argv)
        configure_logging(args.verbose)
        prepare_database(args)
        LOGGER.info(
            "Ground Truth database preparation completed successfully."
        )
        return 0
    except argparse.ArgumentError:
        return 1
    except ValueError as err:
        LOGGER.error("%s", err)
        return 1
    except FileNotFoundError as err:
        LOGGER.error("%s", err)
        return 1
    except ConnectionError as err:
        LOGGER.error("%s", err)
        return 2
    except RuntimeError as err:
        LOGGER.error("%s", err)
        return 3
    except Exception as err:  # pragma: no cover - unexpected issues
        LOGGER.exception("Unexpected error: %s", err)
        return 99


if __name__ == "__main__":  # pragma: no cover
    sys.exit(main())
