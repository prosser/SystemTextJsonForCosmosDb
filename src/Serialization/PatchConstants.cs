namespace SystemTextJsonForCosmosDb.Serialization;

internal static class PatchConstants
{
    public static string ToEnumMemberString(this PatchOperationType patchOperationType)
    {
        return patchOperationType switch
        {
            PatchOperationType.Add => OperationTypeNames.Add,
            PatchOperationType.Remove => OperationTypeNames.Remove,
            PatchOperationType.Replace => OperationTypeNames.Replace,
            PatchOperationType.Set => OperationTypeNames.Set,
            PatchOperationType.Increment => OperationTypeNames.Increment,
            PatchOperationType.Move => OperationTypeNames.Move,
            _ => throw new ArgumentException($"Unknown Patch operation type '{patchOperationType}'."),
        };
    }

    public static class OperationTypeNames
    {
        public const string Add = "add";
        public const string Increment = "incr";
        public const string Move = "move";
        public const string Remove = "remove";
        public const string Replace = "replace";
        public const string Set = "set";
    }

    public static class PatchSpecAttributes
    {
        public const string Condition = "condition";
        public const string Operations = "operations";
    }

    public static class PropertyNames
    {
        public const string From = "from";
        public const string OperationType = "op";
        public const string Path = "path";
        public const string Value = "value";
    }
}