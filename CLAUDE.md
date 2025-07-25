# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SimpleFileSearch is a desktop file search application built with .NET 8 and Avalonia UI. It performs real-time file searching without indexing, supporting both filename pattern matching and content searching within files.

## Development Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run in development mode
dotnet run

# Build for release
dotnet build -c Release

# Publish self-contained executable for Windows
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Testing
The project currently does not have automated tests configured.

## Architecture

### Core Components

**MainWindow.axaml.cs** (`MainWindow.axaml.cs:16`)
- Main UI controller handling user interactions and search orchestration
- Manages search history, settings persistence, and UI state
- Coordinates between UI elements and search engines

**FileSearcher2.cs** (`FileSearcher2.cs:13`) 
- Primary search engine using producer-consumer pattern with BlockingCollection
- Implements parallel file processing with configurable worker threads
- Handles both filename pattern matching and content searching
- Uses CancellationToken for search cancellation support

**Settings System**
- `Settings.cs`: Static settings manager with JSON serialization
- `SettingsContent.cs`: Settings data model containing search history, UI state, and user preferences
- Settings stored in `%APPDATA%\SimpleFileSearch\settings.json`

### Search Architecture

The application uses a two-phase search approach:

1. **File Discovery**: Recursively finds files matching filename patterns using `Directory.GetFiles()`
2. **Content Processing**: Multi-threaded content searching using producer-consumer pattern

Key search features:
- Multiple filename patterns separated by semicolons
- Case-sensitive/insensitive content search
- Accent-aware searching (configurable)
- File size limits for content searching
- Parallel processing with configurable thread count

### UI Architecture

Built on Avalonia UI framework with AXAML markup:
- `MainWindow.axaml`: Main window layout with search controls and results grid
- `App.axaml`: Application-level styling and resources
- Split-panel interface with resizable sections
- Auto-complete search history in combo boxes

### Data Flow

1. User inputs search criteria in MainWindow
2. MainWindow validates inputs and calls FileSearcher2
3. FileSearcher2 discovers files and queues them for processing
4. Worker threads process files in parallel, checking content if needed
5. Results are collected and returned to MainWindow for display
6. UI updates in real-time via status callbacks

## Development Notes

### Framework Details
- Target: .NET 8.0
- UI Framework: Avalonia 11.1.3 with Fluent theme
- Nullable reference types enabled
- Implicit usings enabled

### Current TODO Items (from README.md)
- Allow removal from search history
- Checkbox to ignore accents (partially implemented)
- Textbox to limit file size for content search (implemented)
- Add cancel button during searches (in progress)

### Legacy Code
The codebase contains some legacy Windows Forms remnants (MainWindow.cs, MainWindow.Designer.cs, MainWindow.resx) that are excluded from compilation but remain in the project structure.