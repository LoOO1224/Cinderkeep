Cinderkeep Disabled Scripts Guide
=================================

Purpose
-------
This folder stores scripts that are intentionally disabled during branch merge work.
Unity does not compile files ending with .cs.disabled.

Current Use
-----------
- Assets/_Disabled/Scripts/Cinderkeep contains the previous !! Play Our Game gameplay scripts.
- Main_Lobby, character select, workspace, and hub BGM scripts are still active.

Move Rule
---------
- Move the .cs file and its .meta file together.
- Rename ScriptName.cs to ScriptName.cs.disabled.
- Rename ScriptName.cs.meta to ScriptName.cs.disabled.meta.

Restore Rule
------------
- Rename ScriptName.cs.disabled back to ScriptName.cs.
- Rename ScriptName.cs.disabled.meta back to ScriptName.cs.meta.
- Move both files back to the active Scripts folder.
- Open Unity and verify compile errors before merging.

Important
---------
Do not delete scripts from this folder until the team decides whether to merge, rewrite, or remove them permanently.
