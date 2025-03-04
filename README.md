# pkd-crestron-avf
- .NET framework for Crestron AV control systems.
- Currently uses .NET 8.0

## Software Design
### Plugin-based architecture
- This framework provides an API for hardware and user interface library implementations.
- The overall structure uses a plugin-based system where dependencies are created and injected during boot
- All libraries (DLLs) are expected to be in the /user/ directory of the control system or VC4 room.

### Task Services
#### Application Service
- State management
- Intermediary between user interface events and sending commands to the hardware service
- Base classes can be extended and overriden for advanced or custom requirement

#### Configuration & Domain Services
- The configuration service reads a JSON file (see documentation for examples) and creats a domain data
object used when creating device connections.

#### Hardwar Service
- Creates device control objects from plugins and manages connection to hardware.

#### UI Service
- Creates and manages response/updates to all user interfaces defined in the configuration JSON file.
- Manages the Crestron Fusion connection & error reporting.
