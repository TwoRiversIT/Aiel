### New Rules

Rule ID    | Category                    | Severity | Notes
-----------|-----------------------------|----------|---------------------------------------------
TRSG0001   | Aiel.StrongIds.Generators | Error    | Strong ID declarations must be partial record types.
TRSG0002   | Aiel.StrongIds.Generators | Error    | Strong ID declarations must not use positional record syntax.
TRSG0003   | Aiel.StrongIds.Generators | Error    | Strong ID declarations must implement IStrongId<TValue>.
TRSG0004   | Aiel.StrongIds.Generators | Error    | Strong ID declarations must not declare their own Value member.
TRSG0005   | Aiel.StrongIds.Generators | Error    | Strong ID declarations must not declare instance constructors.
TRSG0006   | Aiel.StrongIds.Generators | Error    | Strong ID declarations use an unsupported backing type.
