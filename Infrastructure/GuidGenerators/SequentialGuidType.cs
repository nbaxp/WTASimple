﻿namespace WTA.Infrastructure.GuidGenerators;

public enum SequentialGuidType
{
    /// <summary>
    /// MySQL,PostgreSQL
    /// </summary>
    SequentialAsString,

    /// <summary>
    /// Oracle
    /// </summary>
    SequentialAsBinary,

    /// <summary>
    /// SQLServer
    /// </summary>
    SequentialAtEnd
}