# Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose

This mod aims to introduce a dynamic election system to RimWorld, allowing players to simulate elections within their colonies. The election system enhances the storytelling and political dynamics within the game by allowing colonists to run for leadership positions. This mod offers a deeper layer of interaction between colonists and adds meaningful choices for players in managing their colony's social hierarchy.

## Key Features and Systems

- **Election System:** The core feature of the mod, introducing an election process where colonists can become candidates and voters select their leaders.
- **Candidate Qualification:** Systems that determine colonists' eligibility to run for office based on custom criteria.
- **Voting Mechanism:** A detailed voting process which calculates votes based on colonist preferences and election rules.
- **Dynamic Social Hierarchy:** Elected leaders introduce buffs, policies, or social dynamics that affect the colony.
- **Ability Effects:** Custom abilities or actions influenced by the leadership and election outcomes.

## Coding Patterns and Conventions

- **Class Naming:** Classes are prefixed with their role or feature, e.g., `CompAbilityEffect_CheckAssist` for ability effects and `VoteModExtension` for mod-specific extensions.
- **Method Visibility:** Methods are generally private or protected to encapsulate functionality, except for those necessary for external interaction.
- **Pascal Case:** Follows PascalCase naming convention for all classes and public methods.
- **Descriptive Methods:** Method names clearly indicate their functionality, e.g., `getVoteNumOfPawn`, `checkLeaderGenderRule`.

## XML Integration

- XML integration allows for defining new abilities, traits, and events pertinent to elections and leadership.
- Ensure new definitions in XML files match their counterparts in C# classes for seamless data binding.

## Harmony Patching

The mod employs Harmony to patch RimWorld functionality, allowing modifications of the game's core mechanics:

- **Patch Implementation:** Harmony patches are implemented in `HarmonyPatches.cs`, targeting specific methods in the game's API.
- **Patch Naming:** Patches are named according to the method or system they alter for clarity, e.g., `patch_SocialCardUtility_DrawPawnRoleSelection`.
- **Order of Execution:** Ensure patch priority aligns with intended modifications by setting priorities and before/after directives where necessary.

## Suggestions for Copilot

1. **Code Completion:** When writing methods related to election systems, suggest implementing patterns seen in existing methods like `get_systemEffectVote`.
2. **Harmony Integration:** Suggest common patching patterns for modifying game functionalities and patterns for compatibility with future game updates.
3. **XML Definitions and Integration:** Propose XML schema definitions for new types introduced by the mod and ensure proper serialization using `ExposeData`.
4. **Performance Optimization:** Offer optimization suggestions for complex computations, such as vote tallying during elections.
5. **Error Handling:** Recommend robust error handling and logging practices to manage unexpected mod interactions.

By leveraging these guidelines, Copilot can cater to the specific needs of the mod, ensuring consistent, efficient, and coherent development.
