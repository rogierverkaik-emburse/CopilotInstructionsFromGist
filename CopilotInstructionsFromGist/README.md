# Copilot Instructions from Gist

Sync `copilot-instructions.md` from a public GitHub Gist into your repository’s `.github` folder automatically.

---

## Overview

This Visual Studio extension allows you to define a single public GitHub Gist as the source of truth for your Copilot instructions.

When enabled, the extension:

- Downloads `copilot-instructions.md` from the configured public Gist
- Writes it to `.github/copilot-instructions.md`
- Only updates the file if content has changed
- Optionally syncs automatically when a solution is opened

The Gist is never modified by this extension.  
Synchronization is strictly one-way: **Gist → Repository**.

---

## Features

- Public Gist support (no authentication required)
- One-way sync only
- Auto-sync on solution open (optional)
- Manual sync command
- Status bar progress indicator
- No unnecessary file overwrites
- No popup interruptions during auto-sync

---

## Installation

1. Build the project in **Release** mode.
2. Locate the generated `.vsix` file in: bin\Release\
3. Double-click the `.vsix` file to install.
4. Restart Visual Studio.

---

## Configuration

1. Open Visual Studio.
2. Go to: Tools → Options → Copilot Gist Sync
3. Provide:
- **Gist URL** (public GitHub Gist containing `copilot-instructions.md`)
- Enable **Auto Sync on Solution Open** (optional)

4. Click **OK**.

---

## Usage

### Manual Sync

Go to: Tools → Sync Copilot Instructions

This will download the Gist and update: .github/copilot-instructions.md if needed.

---

### Automatic Sync

If enabled in settings:

- Sync runs automatically when a solution is opened.
- Status is shown in the Visual Studio status bar.
- No popups are displayed.
- No action is taken if:
  - Auto Sync is disabled
  - No Gist URL is configured
  - The file is already up to date

---

## Requirements

- Visual Studio 2022 (17.x)
- Public GitHub Gist containing a file named: copilot-instructions.md

---

## Behavior Details

- If `.github` does not exist, it will be created.
- If `copilot-instructions.md` does not exist, it will be created.
- If the file exists but content differs, it will be updated.
- If content is identical, no changes are made.

---

## Design Principles

- Source of truth lives in GitHub
- Repository remains clean
- No unnecessary Git diffs
- No blocking UI
- Minimal user interruption

---

## License

Add a license of your choice (for example, MIT).