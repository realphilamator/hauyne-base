================================================================
  ADDING SUBTITLES — Hauyne Base
================================================================

----------------------------------------------------------------
  OVERVIEW
----------------------------------------------------------------

Subtitles are driven by two things:
  1. A key on the ManagedAudioSource component
  2. A matching entry in the localization JSON file

----------------------------------------------------------------
  STEP 1 — FIND OR ADD THE AUDIOSOURCE
----------------------------------------------------------------

Locate the GameObject that plays the sound you want to caption.
It should already have a ManagedAudioSource component. If it
doesn't, add one — it will appear automatically when you add
an AudioSource.

Set the Audio Type to either SFX or Voice. Music sources do
not support subtitles.

----------------------------------------------------------------
  STEP 2 — FILL IN THE SUBTITLE FIELDS
----------------------------------------------------------------

With the Audio Type set to SFX or Voice, the following fields
will appear in the Inspector:

  Subtitle Key  — a unique string identifier for this sound
                  Follow the naming convention below.
  Sub Duration  — how many seconds the subtitle stays on screen
  Subtitle Color — text color (default white)
  Positional    — if ticked, the subtitle orbits the HUD edge
                  pointing toward the sound source in world space.
                  Leave unticked for UI sounds or dialogue.

----------------------------------------------------------------
  STEP 3 — ADD THE KEY TO THE JSON
----------------------------------------------------------------

There are two ways to do this:

  OPTION A — Localization Editor (recommended)
    Open the editor via:
      Unity menu → Tools → Subtitle Localization Editor

    Select your target language from the language bar at the
    top of the window. If the language doesn't exist yet,
    click "+ New Language" and enter the language code (e.g.
    fr, de, ja).

    Click "+ Add Entry" and fill in:
      Key:   Your_Subtitle_Key
      Value: *Your caption text*

    Click Save. Done.

  OPTION B — Edit the JSON directly
    Open:
      Assets/StreamingAssets/subtitles_en.json

    Or for other languages:
      Assets/StreamingAssets/subtitles_fr.json
      Assets/StreamingAssets/subtitles_de.json
      etc.

    Add a new entry at the bottom of the "localizations" array:

      { "key": "Your_Subtitle_Key", "value": "*Your caption text*" }

In either case, make sure the key exactly matches what you
typed in the ManagedAudioSource Inspector.

----------------------------------------------------------------
  STEP 4 — PLACE THE SUBTITLE PREFAB
----------------------------------------------------------------

The subtitle UI prefab is located at:

  Assets/Prefabs/Subtitle.prefab

Ensure the SubtitleManager in your scene has this prefab
assigned in its Subtitle Prefab field. If you are working in
an existing scene it should already be set up — you do not
need to change anything.

----------------------------------------------------------------
  SUBTITLE KEY NAMING CONVENTION
----------------------------------------------------------------

Use the format:  Category_Object_Action

  Sfx_Doors_StandardOpen    → *Door opens*
  Sfx_Elv_Running           → *Elevator noises*
  Voice_Baldi_Greeting      → Hello!
  Voice_Principal_Warning   → No running in the halls!

Keeping keys consistent makes the JSON easy to search and
maintain across contributors.

----------------------------------------------------------------
  POSITIONAL vs NON-POSITIONAL
----------------------------------------------------------------

  Positional (ticked):
    Subtitle orbits the screen edge pointing toward the sound.
    Best for enemies, hazards, or any off-screen audio.

  Non-positional (unticked):
    Subtitle appears at the bottom center of the screen.
    Best for UI sounds, music stings, or cutscene dialogue.

================================================================