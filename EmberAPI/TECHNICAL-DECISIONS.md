# Technical Decisions - EmberAPI

## Active Decisions

### TD-001: Remove PostBuildEvent xcopy
- **Date**: 2026-01-10
- **Status**: Accepted
- **Context**: PostBuildEvent was trying to copy from non-existent `$(ProjectDir)$(PlatformName)` directory causing MSB3073 error
- **Decision**: Remove the PostBuildEvent entirely as the directory doesn't exist
- **Consequences**: 
  - ? Build succeeds without errors
  - ? May need alternative deployment strategy if files need copying
- **Alternatives Considered**: 
  - Create the missing directory (rejected: unclear what should be in it)
  - Fix the path (rejected: no clear correct path exists)

### TD-002: Documentation Standards
- **Date**: 2026-01-10
- **Status**: Accepted
- **Context**: Need standardized documentation across all projects
- **Decision**: Implement CHANGELOG.md, COPILOT-CHANGELOG.md, and TECHNICAL-DECISIONS.md
- **Consequences**: Better traceability and decision documentation
