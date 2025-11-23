# ThinkJavaRewrite

<div align="center">

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-black?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![NodeCanvas](https://img.shields.io/badge/NodeCanvas-FSM-FF6B6B?style=for-the-badge)
![DOTween](https://img.shields.io/badge/DOTween-Animation-00D9FF?style=for-the-badge)
![Easy Save 3](https://img.shields.io/badge/Easy%20Save%203-4CAF50?style=for-the-badge)

</div>



## About

ThinkJavaRewrite is an educational 2D platformer game built in Unity that teaches Java programming concepts through interactive gameplay. The game features a comprehensive student management system that integrates with the [ThinkJavaDjangoWeb](https://github.com/keentooyyy/ThinkJavaDjangoWeb) Django web backend control panel, allowing teachers to track student progress, manage levels, and analyze performance data in real-time.

### Key Features

- **Educational Gameplay**: Learn Java programming concepts through interactive platformer levels
- **Multi-Level System**: Tutorial, Level 1, and Level 2 with progressive difficulty
- **Student Authentication**: Secure login system integrated with Django backend
- **Progress Tracking**: Real-time save/load system with cloud synchronization
- **Achievement System**: Unlock achievements as you progress through the game
- **Dialogue System**: Interactive dialogue with typewriter effects and character interactions
- **Puzzle Mechanics**: Pickup puzzles and interactive elements to solve challenges
- **Enemy AI**: Waypoint-based enemy movement with head-stomp mechanics
- **Player Mechanics**: Jumping, climbing, movement with grounded detection
- **UI System**: Modern UI with loading screens, progress bars, and level selection

## Prerequisites

Before opening the project, ensure you have the following installed:

- **[Unity Hub](https://unity.com/download)** (Recommended for managing Unity versions)
- **[Unity Editor 2022.3.62f2](https://unity.com/releases/editor/archive)** (Exact version required)
- **Visual Studio 2022** or **JetBrains Rider** (C# IDE for scripting)
- **Git** (for version control)
- **[ThinkJavaDjangoWeb Backend](https://github.com/keentooyyy/ThinkJavaDjangoWeb)** (Required for cloud save and authentication features)

> **Note:** The Django backend must be set up and running for the game's authentication and cloud save features to work. See the [ThinkJavaDjangoWeb repository](https://github.com/keentooyyy/ThinkJavaDjangoWeb) for backend setup instructions.

You can verify installation with:

```bash
unity --version
git --version
```

## Project Setup

### Step 1: Clone the Repository

Clone the project to your local machine:

```bash
git clone https://github.com/keentooyyy/ThinkJavaRewrite.git
cd ThinkJavaRewrite
```

### Step 2: Open in Unity

1. Open **Unity Hub**
2. Click **Add** and select the `ThinkJavaRewrite` folder
3. Unity will detect the project version (2022.3.62f2)
4. Click **Open** to launch the project

> **Note:** If you don't have Unity 2022.3.62f2 installed, Unity Hub will prompt you to download it.

### Step 3: Configure API Endpoint

The game connects to the **[ThinkJavaDjangoWeb](https://github.com/keentooyyy/ThinkJavaDjangoWeb)** Django backend for student authentication and progress tracking. 

**First, set up the Django backend:**
1. Follow the setup instructions in the [ThinkJavaDjangoWeb repository](https://github.com/keentooyyy/ThinkJavaDjangoWeb)
2. Ensure the Django server is running and accessible
3. Note your API endpoint URL (e.g., `http://localhost:8000/api/` for local development)

**Then, configure the Unity game:**
1. Open the **MainMenu** scene (`Assets/Scenes/MainMenu.unity`)
2. Find the **LoginFSM** GameObject in the hierarchy
3. In the Inspector, locate the **FSM Owner** component
4. Set the `apiURL` blackboard variable to your Django API endpoint:
   ```
   http://localhost:8000/api/  (for local development)
   https://your-api-domain.com/api/  (for production)
   ```

Alternatively, you can set it programmatically in code:

```csharp
GameSaveAPIManager.SetAPIBaseUrl("http://localhost:8000/api");
```

## Project Structure

```
ThinkJavaRewrite/
├── Assets/
│   ├── Animations/          # Animation controllers and clips
│   │   ├── Player/          # Player animations
│   │   ├── Enemies/         # Enemy animations
│   │   └── Checkpoint/      # Checkpoint animations
│   ├── Dialogues/           # Dialogue ScriptableObjects
│   │   ├── Tutorial/        # Tutorial dialogues
│   │   ├── Level1/          # Level 1 dialogues
│   │   └── Level2/          # Level 2 dialogues
│   ├── FSM/                 # NodeCanvas FSM system
│   │   ├── Scripts/         # C# scripts for FSM actions/conditions
│   │   │   ├── Actions/     # FSM action nodes
│   │   │   ├── Conditions/ # FSM condition nodes
│   │   │   ├── Progress/   # Save/load and progress management
│   │   │   ├── UI/          # UI management scripts
│   │   │   └── Dialogue/   # Dialogue system scripts
│   │   └── *.asset          # FSM graph files
│   ├── Scenes/              # Unity scenes
│   │   ├── MainMenu.unity
│   │   ├── LoadingScene.unity
│   │   ├── TutorialScene.unity
│   │   ├── Level1Scene.unity
│   │   └── Level2Scene.unity
│   ├── Sprites/             # Game sprites and prefabs
│   ├── Puzzle Configs/      # Puzzle configuration assets
│   ├── Plugins/             # Third-party plugins
│   │   ├── ParadoxNotion/   # NodeCanvas FSM
│   │   ├── Demigiant/       # DOTween
│   │   └── Easy Save 3/     # Save system
│   └── TextMesh Pro/        # TextMeshPro resources
├── ProjectSettings/         # Unity project settings
└── Packages/                # Unity package dependencies
```

## Key Systems

### Player System

- **Movement**: Horizontal movement with acceleration/deceleration
- **Jumping**: Grounded detection with jump mechanics
- **Climbing**: Climbable surfaces with vertical movement
- **Health**: HP system with damage on collision
- **Animation**: State-based animations for idle, run, jump, climb

### Enemy System

- **Waypoint Movement**: Enemies patrol between waypoints
- **Head Stomp**: Players can defeat enemies by jumping on them
- **Death Animation**: Enemy death sequences
- **Global Management**: Centralized enemy tracking and management

### Dialogue System

- **Typewriter Effect**: Character-by-character text reveal
- **Skip Functionality**: Fast-forward dialogue with button press
- **Auto-Trigger**: Dialogue triggers automatically when conditions are met
- **Sequence Management**: Multiple dialogue lines in sequence

### Progress System

- **Cloud Save**: Integration with Django backend for progress sync
- **Level Tracking**: Unlock status, completion times, best times
- **Achievement System**: Unlockable achievements with tracking
- **Local Backup**: Easy Save 3 for local save data

### UI System

- **Loading Screen**: Async scene loading with progress bar
- **Level Selection**: UI for selecting and viewing level progress
- **Profile Display**: Student profile information display
- **Score Counter**: Animated score display with DOTween
- **Star Reveal**: Achievement star reveal animations

## Building the Game

### Development Build

1. Go to **File > Build Settings**
2. Select your target platform (Windows, Mac, Linux, WebGL, etc.)
3. Click **Build** or **Build and Run**

### Production Build

For production builds, ensure:

- **API Endpoint** is configured correctly
- **Build Settings** have all required scenes added
- **Player Settings** have appropriate company/product name
- **Quality Settings** are optimized for target platform

## Configuration

### Changing API Endpoint

The API endpoint can be configured in multiple ways:

**Method 1: Unity Inspector**
- Open `MainMenu` scene
- Select `LoginFSM` GameObject
- Set `apiURL` in FSM blackboard

**Method 2: Code**
```csharp
GameSaveAPIManager.SetAPIBaseUrl("https://your-api-domain.com/api");
```

**Method 3: Runtime**
- The game will attempt to read from LoginFSM blackboard if not set programmatically

### Input Configuration

Input is managed through Unity's Input System:

- **MainInputConfig.asset**: Main input configuration
- Button names: `ActionA`, `ActionB`, etc.
- Can be customized in the Input System window

## Development Notes

- **FSM System**: Game logic is primarily managed through NodeCanvas FSM graphs
- **Hot Reload**: Script changes are automatically compiled and reloaded in Play Mode
- **Save System**: Uses Easy Save 3 for local saves, custom API for cloud saves
- **Async Operations**: Coroutines used for API calls and async scene loading
- **Event System**: Custom event system for decoupled communication
- **DOTween**: All animations use DOTween for smooth, performant tweening

## API Integration

The game communicates with the **[ThinkJavaDjangoWeb](https://github.com/keentooyyy/ThinkJavaDjangoWeb)** Django backend control panel for:

- **Student Login**: `POST /api/student_login/` - Authenticate students and retrieve profile data
- **Download Progress**: `GET /api/progress/<id>/` - Download saved game progress from server
- **Upload Progress**: `POST /api/progress/update/<id>/` - Upload game progress to server

> **Note:** The Django backend must be running and accessible for the game to function with cloud save features. See the [ThinkJavaDjangoWeb repository](https://github.com/keentooyyy/ThinkJavaDjangoWeb) for backend setup instructions.

### API Response Format

```json
{
  "status": "success",
  "student": {
    "id": 1,
    "student_id": "17-2168-338",
    "first_name": "John",
    "last_name": "Doe",
    "role": "student"
  },
  "section": {
    "dept": "CS",
    "year_level": 2,
    "section_letter": "A",
    "full_section": "CS-2A"
  },
  "profile": { ... },
  "test_status": {
    "has_taken_pretest": false,
    "has_taken_posttest": false,
    "all_levels_completed": false,
    "can_take_posttest": false
  }
}
```

## Troubleshooting

### API Connection Issues

If you encounter API connection errors:

1. **Check Backend Status**: Ensure the [ThinkJavaDjangoWeb](https://github.com/keentooyyy/ThinkJavaDjangoWeb) Django backend is running
2. **Check API URL**: Verify the endpoint is correct in LoginFSM blackboard (should match your Django server URL)
3. **Check Network**: Ensure internet connection is active and backend is accessible
4. **Check CORS**: Django backend must allow Unity WebGL requests (if building for WebGL). Configure CORS settings in Django settings.py
5. **Check Backend Logs**: Review Django server logs for API request errors
6. **Check Unity Logs**: View Unity Console for detailed error messages

### Build Errors

If build fails:

1. **Check Dependencies**: Ensure all packages are imported correctly
2. **Check Scenes**: Verify all scenes are added to Build Settings
3. **Check Scripts**: Resolve any compilation errors in Console
4. **Clean Build**: Delete `Library` folder and rebuild (will reimport assets)

### Save Data Issues

If save/load doesn't work:

1. **Check Permissions**: Ensure file write permissions on target platform
2. **Check API**: Verify API endpoint is accessible and returns correct format
3. **Check Logs**: Review Unity Console for save/load errors
4. **Check Format**: Verify JSON format matches expected structure

## Tech Stack

- **Game Engine**: Unity 2022.3.62f2
- **Scripting**: C# (.NET Framework)
- **FSM System**: NodeCanvas (Paradox Notion)
- **Animation**: DOTween (Demigiant)
- **Save System**: Easy Save 3 (Moodkie)
- **UI Framework**: Unity UI (uGUI) + TextMeshPro
- **Rendering**: Universal Render Pipeline (URP)
- **Input System**: Unity Input System
- **Audio**: Unity Audio System

## Dependencies

### Required Packages

- **NodeCanvas**: FSM and behavior tree system
- **DOTween**: Animation and tweening library
- **Easy Save 3**: Local save/load system

### Unity Packages

- **Input System**: Modern input handling
- **Universal RP**: 2D rendering pipeline
- **2D Sprite**: 2D sprite support
- **Timeline**: Cutscene and animation sequencing

## License

This project is for **personal use only**. Any commercial use is not the responsibility of the project maintainer. Users must ensure they have proper rights and licenses for all assets, libraries, and dependencies used in this project. The project maintainer assumes no liability for any misuse or unauthorized use of this software.
