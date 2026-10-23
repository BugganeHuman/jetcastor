# Jetcaster

**Jetcaster** is a simple and convenient backup helper designed to make backing up your files as quick and effortless as possible.

## How It Works

The main feature of Jetcaster is **Fast Backup**. Everything else in the application is built around supporting this functionality.

The application includes:

- **Fast Backup** — quickly back up your configured files and directories.
- **Path Management** — add, view, update, and delete paths using CRUD operations.
- **Settings** — customize backup options, including minimum free disk space requirements.

All paths and settings are stored locally in an **SQLite database**.

## Why Jetcaster?

### Simple and Convenient Interface

Jetcaster features a clean, interactive console interface powered by Spectre.Console.

Almost everything can be controlled using your keyboard's arrow keys and Enter, making navigation quick and intuitive.

### A Time-Tested Idea

Jetcaster is essentially the second version of an older backup helper project.

The original application proved that the Fast Backup concept is genuinely useful in everyday life. After years of use, the idea has demonstrated its convenience and practicality.

The main advantage is simple: **you don't need to spend time configuring and launching backups every single time.**

Just spend around 10 minutes setting up your paths once.

After that, your backup routine becomes as simple as:

1. Open Jetcaster.
2. Press Enter.
3. Let the application handle the backup.

That's it.

### Easy Migration

Moving between installations is straightforward.

All your paths and settings are stored in a single SQLite database file.

Simply keep a copy of that file, and you can transfer your configuration to another installation without setting everything up again.

## Tech Stack

- **C#**
- **.NET**
- **Spectre.Console** — interactive console interface
- **SQLite** — local storage for paths and settings

---

started 13.10.2026