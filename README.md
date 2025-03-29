# MVVMPaintApp

A WPF-based paint application developed using the MVVM (Model-View-ViewModel) architecture pattern. This project was created for the Visual Programming (IT008) course.

## Team Members

- Bùi Xuân Minh (2352091823520918@gm.uit.edu.vn)
- Nguyễn Đức Nhân (2352107823521078@gm.uit.edu.vn)
- Thang Tuấn Phát (2352115023521150@gm.uit.edu.vn)

## Features

- Drawing tools (brush, pencil, shapes)
- Layer manipulation and management
- UI components for easy navigation and control
- File conversion and export capabilities

## Project Structure

- **Converters**: Custom value converters for UI enhancements
- **Interfaces**: Abstraction layer for dialog management and UI components
- **Models**: Core data structures for layer manipulation
- **Resources**: Shared resources for dialog management and UI components
- **Services**: Service layer for application functionality
- **UserControls**: Custom UI controls with data binding capabilities
- **ViewModels**: MVVM view models connecting the UI to the application logic
- **Views**: User interface components with layer manipulation capabilities

## Technical Implementation

The application leverages several WPF features, including:
- Data binding
- Commands
- Templates
- Styles
- Resources
- XAML-based UI definitions

## Screenshots
Dashboard
![image](https://github.com/user-attachments/assets/0848b6dd-2487-4c44-ba5f-27c8e8526062)

New file
![image](https://github.com/user-attachments/assets/cf0f6ff5-6f45-4098-b72e-e08278bb2dae)

Main canvas
![image](https://github.com/user-attachments/assets/87a4c924-d14d-4eb6-8a51-a35b443b48c7)

Credits
![image](https://github.com/user-attachments/assets/7c281699-b510-49a5-b6cf-8275a4fb908f)

## Keyboard Shortcuts

These shortcuts are implemented

| Shortcut | Action | Description |
|----------|--------|-------------|
| Ctrl+Z | Undo | Undo last action |
| Ctrl+Y | Redo | Redo last action |
| V | Pencil Tool | Switch to Pencil tool |
| B | Brush Tool | Switch to Brush tool |
| E | Eraser Tool | Switch to Eraser tool |
| F | Fill Tool | Switch to Fill tool |
| Q | Color Picker | Switch to Color Picker tool |
| Z | ZoomPan | Switch to Zoom/Pan tool |
| Esc | Default Tool | Switch to Default tool |
| Delete | Delete Layer | Delete the currently selected layer |

## Requirements

- Windows operating system
- .NET Framework (version used in development)
- Visual Studio (recommended for development)

## How to Run

1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution
4. Run the application
