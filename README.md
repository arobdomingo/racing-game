# Unity Racing Game

A personal C# / Unity learning project working toward a single-player racing MVP.
Current state: vehicle demo baseline; race progression and a playable circuit are
not implemented. Editor validation of the baseline changes is pending.

## Setup and controls

1. Install Unity **6000.1.0f1** through Unity Hub and open this directory.
2. Allow Unity to restore the packages in Packages/manifest.json.
3. Open **Assets/F1/F1 Demo.unity** and enter Play Mode.
4. Focus the Game view. **P** accelerates, **O** brakes, **A/D** steer,
   **comma/period** shift down/up through eight forward gears.

F1 Demo is the enabled build scene. SampleScene remains available as an untouched
sample. Use File > Build Profiles to select your installed desktop target and build.
No verified release build or gameplay screenshots are available yet.

See the [repository audit and roadmap](docs/MVP-AUDIT.md) and
[engineering notes and manual tests](docs/ENGINEERING-NOTES.md).
The Formula One Car asset metadata identifies an external asset source; confirm
license and redistribution terms before publishing a release. No project-wide
license is asserted here.
