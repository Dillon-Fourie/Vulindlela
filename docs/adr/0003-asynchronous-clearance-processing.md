# ADR 0003: Process Clearance Documents Asynchronously

# Context

The Clearance service must support citizens by allowing the submission of documents in advance for processing. Processing a single document may take several seconds and the system should support asynchronous processing.

It would not be practical to make citizens wait for the whole processing operation to finish and then return a response. Processing many submissions in parallel may result in the Clearance API becoming unresponsive to other requests.

The Phase 1 requirements mentioned the use of System.Threading.Channels and BackgroundService to implement Background processing.

# Decision

The Clearance service will use System.Threading.Channels to implement an in-memory queue to process documents asynchronously.

When a citizen submits a document, the API will add a processing job to the queue and provide a response to the citizen indicating that the document has been accepted for processing.

Background processing will be implemented using ASP.NET Core BackgroundService. The service will maintain a dependency injection scope for EF Core and other scoped services.

Upon completion, the background worker will update the status of the processed document in the Clearance service database.

# Alternatives considered

Synchronous document processing within the HTTP request was considered. Rejected because the citizen would have to wait for the entire processing operation to complete. This would keep HTTP requests open unnecessarily and negatively impact the API responsiveness, particularly for high document submission volumes.

Using Task.Run to process documents in the background was considered. Rejected because, from an API request, starting background tasks provides no managed processing queue and unreliable control of tasks during shutdown of the application. This makes tracking and controlling pending jobs difficult.

Considering the use of a message broker such as RabbitMQ was considered. This was rejected for Phase 1 because it would bring additional complexity to the existing system, including additional infrastructure, configuration, and deployment. The assignment can demonstrate the required asynchronous processing without introducing a message broker.

Using an in-memory Channel and Background Service was selected because it meets Phase 1 requirements and uses built-in .NET features to create a managed asynchronous queue.

# Consequences

What we gain. 
Responses are provided to citizens without having to wait for the completion of the document processing operation. Independently processing the document requests allows the worker to catch up on the requested jobs. The required .NET framework for asynchronous processing is utilized.

What we accept. 
Since the queue is in memory, it is not durable. Jobs that have been accepted but not processed can be lost if the application stops or crashes. In order to alleviate concern for citizens, the system should maintain the status of the processing so citizens can determine the status of their submissions. Background processing introduces added complexity for dependency injection, error handling and management of the worker.

The costs described above are accepted in order to keep the Phase 1 implementation simple and demonstrate asynchronous processing of document requests.