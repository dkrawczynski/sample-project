# LSC_CofRoHSReport — Infor CSI IDO Extension

Certificate of RoHS shipment report implemented as an Infor CSI (Mongoose IDO framework) extension class.

## Overview

This IDO extension replaces the `Rpt_LSC_CofRoHSReportSp` stored procedure with inline C# query logic. It returns shipment details for a given customer order line/release, including item, revision, ship date, quantity, and customer information.

## Files

| File | Description |
|------|-------------|
| `LSC_CofRoHSReportExtensionClass.cs` | C# IDO extension class |
| `LSC_CofRoHSReport_IDO.xml` | IDO definition — import via IDO Developer |
| `LSC_CofRoHSReport_Request.xml` | Sample IDO method invocation request |
| `LSC_CofRoHSReport_Response.xml` | Sample IDO method invocation response (DiffGram) |

## IDO Details

- **IDO Name:** `LSC_CofRoHSReport`
- **Extension Class:** `LSC.Extensions.LSC_CofRoHSReportExtensionClass`
- **Method:** `Rpt_LSC_CofRoHSReportSp`

### Method Parameters

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| CoNum | string | Input | Customer order number |
| CoLine | short | Input | Customer order line |
| CoRelease | short | Input | Customer order release |
| pSite | string | Input | Site |
| Results | DataSet | InputOutput | Returned report rows |
| Infobar | string | Output | Error message (empty on success) |

### Returned Columns

| Column | Type | Source |
|--------|------|--------|
| CoNum | string(10) | co_ship.co_num |
| CoLine | short | co_ship.co_line |
| CoReleaseS | short | co_ship.co_release |
| item | string(30) | coitem.item |
| description | string(40) | item.description |
| rev | string(8) | item.revision |
| curDate | datetime | co_ship.ship_date |
| qty | decimal(19,8) | co_ship.qty_shipped |
| cust_num | string(7) | custaddr.cust_num (bill-to) |
| custName | string(60) | custaddr.name (bill-to) |

## Deployment

1. Build the assembly and deploy the DLL to the IDO Runtime assembly folder
2. Import `LSC_CofRoHSReport_IDO.xml` via IDO Developer → File → Import
3. Verify the extension class mapping and save/compile the IDO
4. Test using the IDO Runtime Tester with a valid CoNum, CoLine, CoRelease, and Site

## Notes

- CoNum is automatically prefixed with `S` if not already present and padded to 10 characters
- Session context and transaction isolation are managed by the IDO runtime
