# Unity Racing Game

A personal C# / Unity learning project working toward a single-player racing MVP.
Current state: driving baseline plus a greybox closed circuit, ordered checkpoint
progress and vehicle recovery. Timed races, results and persistent best laps remain
to be implemented.

## Setup and controls

1. Install Unity **6000.1.0f1** through Unity Hub and open this directory.
2. Allow Unity to restore the packages in Packages/manifest.json.
3. Open **Assets/Scenes/MVP Circuit.unity** and enter Play Mode. The circuit is generated when Play begins.
4. Focus the Game view. **P** accelerates, **O** brakes, **A/D** steer,
   **comma/period** shift down/up through eight forward gears, and **R** recovers
   to the last valid checkpoint.

MVP Circuit is the enabled build scene. F1 Demo remains a driving test scene, and
SampleScene remains available as an untouched sample. Use File > Build Profiles to select your installed desktop target and build.
No verified release build is available yet. A rendered circuit preview is included
in the circuit guide.

See the [circuit implementation and test guide](docs/CIRCUIT-MILESTONE.md),
[repository audit and roadmap](docs/MVP-AUDIT.md) and
[engineering notes and manual tests](docs/ENGINEERING-NOTES.md).
The Formula One Car asset metadata identifies an external asset source; confirm
license and redistribution terms before publishing a release. No project-wide
license is asserted here.
