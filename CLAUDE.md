# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Deploy

No automated build scripts exist. The project requires Infor CSI IDO Runtime assemblies (`Mongoose.IDO`, `Mongoose.IDO.Protocol`) which are not distributed as NuGet packages — they must be copied from the IDO Runtime assembly folder on a machine running Infor CSI.

**Manual deployment steps:**
1. Build the assembly (Visual Studio or `dotnet build` with framework DLLs on the reference path)
2. Deploy the compiled DLL to the IDO Runtime assembly folder on the CSI application server
3. Import `LSC_CofRoHSReport_IDO.xml` via IDO Developer → File → Import
4. Verify the extension class mapping, save, and compile the IDO
5. Test via IDO Runtime Tester with a valid CoNum, CoLine, CoRelease, and Site

## Unit Tests

Tests live in `test/` and cover pure logic that does not depend on the IDO runtime.

```
cd test
dotnet test
```

To run a single test:
```
dotnet test --filter "FullyQualifiedName~CoNumNormalizerTests.Normalize_WhenCoNumLacksSPrefix"
```

## Architecture

**One extension class:** `LSC.Extensions.LSC_CofRoHSReportExtensionClass` (`LSC_CofRoHSReportExtensionClass.cs`)
- Inherits `IDOExtensionClass` (Mongoose.IDO)
- Exposes one IDO method: `Rpt_LSC_CofRoHSReportSp`
- Pre-processes `CoNum` before querying: prefixes `S` if absent (case-insensitive), then pads to `CoNumTypeLength` (10) with `PadRight`
- Executes a parameterized SQL SELECT across 8 joined tables via `Context.Commands.CreateTextCommand` / `ExecuteDataSet`
- Returns severity `0` on success or `16` on exception, with the exception message in the `Infobar` output parameter

**IDO definition (`LSC_CofRoHSReport_IDO.xml`):** declares the IDO name, extension class binding, 10 output properties, and 6 method parameters. Must stay in sync with the C# method signature.

**`pSite` parameter** is declared in the IDO XML and accepted in the C# signature but is not used in the SQL — retained for stored-procedure signature compatibility.

**Sample XML files** (`*_Request.xml`, `*_Response.xml`) show the HTTP request/response format used by the IDO RequestService endpoint.
