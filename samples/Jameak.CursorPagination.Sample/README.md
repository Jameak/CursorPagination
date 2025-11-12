## Files of interest in this sample project

- [KeySetPaginationStrategy.cs](KeySetPaginationStrategy.cs): Configures a class for __KeySet__ pagination source generation.
- [OffsetPaginationStrategy.cs](OffsetPaginationStrategy.cs): Configures a class for __Offset__ pagination source generation.
- [Controllers/PaginatedDataController.cs](Controllers/PaginatedDataController.cs): 3 example endpoints that use KeySet and Offset pagination to implement cursor-pagination as well as pagination via page-number.
- [PaginatedBatchJob.cs](PaginatedBatchJob.cs): An example of a job that needs to process data in paginated batches.

This sample is configured to write the source generated files to disk. These can be found [in this folder](GeneratedFiles/Jameak.CursorPagination.SourceGenerator/Jameak.CursorPagination.SourceGenerator.PaginationGenerator).
