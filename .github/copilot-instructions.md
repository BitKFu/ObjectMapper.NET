# Copilot Instructions

## Project Guidelines
- Für diese Lösung soll die PackageReference auf System.Reflection.Emit nur für netstandard2.0 gelten.
- Für diese Migration bevorzugt der Nutzer möglichst wenige Code-Änderungen und möchte die Retry-Logik in einer neuen ReliableSqlConnection kapseln, statt breite Umbauten vorzunehmen.
- Connect(string connectionString) darf nicht verändert werden, da dort bereits ein vollständiger Connection String übergeben wird.
