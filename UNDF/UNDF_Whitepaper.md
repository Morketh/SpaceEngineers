# UNDF – Unified Navigation & Docking Framework
## System Architecture White Paper

*(Design-first specification – no code)*

---

## 1. Core Philosophy

UNDF is designed around **air-traffic–control principles**, not autonomous drones.

Key axioms:

1. ATC is authoritative  
2. Drones are obedient, state-driven actors  
3. Stations emit events, they do not move drones  
4. No system assumes success without confirmation  
5. Every transition is explicitly ACKed  

The framework prioritizes **determinism, safety, and extensibility**.

---

## 2. System Roles

### 2.1 Drone (Mobile Actor)

A drone is a generic flight platform with:
- Navigation
- Communication
- Local monitoring (cargo, power, equipment)
- A strict internal state machine

Drones never decide:
- When to enter airspace
- When doors open
- Which dock to use
- When it is safe to proceed

They **request** and **acknowledge** only.

---

### 2.2 ATC Node (Traffic Authority)

An ATC node represents controlled airspace.

Responsibilities:
- Airspace ownership
- Traffic sequencing
- Dock assignment
- Waypoint orchestration
- Airlock coordination
- Collision prevention

ATC parses configuration once, caches it, and becomes the sole authority.

---

### 2.3 Station Systems (Event Executors)

Stations:
- Execute physical actions (doors, pistons, timers)
- Signal completion via Event Controllers
- Never assume drone position

---

## 3. Drone Classes

All drones share the same PB skeleton.

Differences are capabilities, not logic.

- Cargo Drone
- Tanker
- Miner
- Trader
- Passenger

---

## 4. Communication Model (IGC)

- Unicast: authoritative commands
- Broadcast: discovery and announcements

Authority rules:
- ATC commands override drone intent
- Drone transitions require ATC ACK
- Stations never command drones

---

## 5. Identity & Persistence

Each drone has a persistent unique ID:
- Generated once per grid
- Stored in PB Storage
- Used for ATC tracking and reservations

---

## 6. State Machines

### Drone States (Simplified)

BOOT → REGISTERING → IDLE → REQUESTING_CLEARANCE → APPROACHING  
→ HOLDING → TRANSITING_WAYPOINTS → DOCKING → DOCKED  
→ TASK_EXECUTION → REQUEST_UNDOCK

Emergency states may interrupt at any point.

---

### ATC Model

ATC tracks:
- Active drones
- Reserved waypoints
- Dock occupancy
- Airlock state
- Traffic locks

ATC reacts to ACKs and events, not ticks.

---

## 7. Waypoint System

Waypoint LCDs represent physical navigation markers.

Properties:
- World position = block position
- Orientation = block orientation
- CustomData = waypoint behavior

Waypoints are parsed and cached by ATC only.

---

## 8. Airlock Handling

UNDF does not implement airlocks.

- External airlock scripts manage doors
- Event Controllers detect completion
- ATC receives explicit READY signals

Airlock states:
- ENTRY_READY
- EXIT_READY

---

## 9. Hold Points & Safety

Before any transition:
1. ATC commands HOLD
2. Drone reaches waypoint
3. Drone sends WAYPOINT_ACK
4. ATC triggers station event
5. Station executes
6. Event Controller confirms
7. ATC authorizes next step

---

## 10. Cargo & Resources

- GOAT handles all inventory movement
- UNDF only observes cargo levels
- Thresholds trigger routing decisions

---

## 11. Routing & Trade Networks

Trading drones:
- Read route LCDs onboard
- Routes reference ATC station names

ATC nodes advertise services and form a network.

---

## 12. ATC Nodes

ATC nodes may be:
- Static stations
- Outposts
- Refineries
- Mobile carriers

Any node with IGC, waypoints, and docks may be ATC.

---

## 13. Data Persistence

- Configuration stored in CustomData
- Parsed once, cached
- Reloaded only on explicit command

---

## 14. Mixins & Architecture

Core mixins:
- Identity
- Messaging
- Parsing
- State handling
- Cargo monitoring

Entry PBs:
- Drone
- ATC
- GUI
- Extensions

---

## 15. Final Principle

**Movement is permission-based.  
Knowledge is centralized.  
Execution is distributed.**
