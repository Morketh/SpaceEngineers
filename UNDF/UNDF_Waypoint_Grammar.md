# UNDF Waypoint Grammar Specification
## Waypoint LCD CustomData Format

*(Authoritative grammar – no code)*

---

## 1. Purpose

Waypoint LCDs define **behavior at a physical point in space**.

They are parsed by ATC and cached as immutable waypoint objects.

---

## 2. General Rules

- One waypoint per LCD
- Parsed only by ATC
- Drones never read waypoint CustomData directly
- All keys are optional unless specified

---

## 3. Grammar Structure

Key-value format:

KEY = VALUE

Comments:

# Comment text

---

## 4. Core Fields

### waypoint_name (required)
Human-readable identifier.

waypoint_name = HANGAR_ENTRY_A

---

### type
Defines waypoint role.

type = HOLD | TRANSIT | DOCK | AIRLOCK | EXIT

---

### speed
Maximum allowed speed at waypoint.

speed = 5.0

---

### offset
Positional offset from LCD block (meters).

offset = 0,2,0

---

### rotation
Desired drone orientation in degrees.

rotation = 0,90,-90

---

## 5. Drone Instructions

### drone_event
Command sent to drone upon arrival.

drone_event = SET_MODE:DOCKING

Multiple allowed.

---

### next_state
Requested drone state transition.

next_state = HOLDING

---

## 6. Station Events

### station_event
Triggers local station action.

station_event = TIMER:HANGAR_A_OPEN

Multiple allowed.

---

## 7. Airlock Integration

### airlock_name
Logical airlock identifier.

airlock_name = HANGAR_A

---

### wait_for
Event required before proceeding.

wait_for = AIRLOCK|HANGAR_A|ENTRY_READY

---

## 8. Traffic Control

### reserve
Reserves waypoint for single drone.

reserve = true

---

### release_on
Condition to release reservation.

release_on = WAYPOINT_ACK

---

## 9. Example Waypoint

waypoint_name = HANGAR_ENTRY_A
type = AIRLOCK
speed = 2.5
offset = 0,1.5,0
rotation = 0,180,0
station_event = TIMER:HANGAR_A_CYCLE
wait_for = AIRLOCK|HANGAR_A|ENTRY_READY
next_state = TRANSITING_WAYPOINTS

---

## 10. Design Principle

Waypoints are **data, not logic**.

ATC interprets.  
Stations execute.  
Drones obey.
