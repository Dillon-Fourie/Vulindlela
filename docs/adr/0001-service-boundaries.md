# ADR 0001: Separate Booking and Clearance into Independent Services

# Context

Vulindlela has to provide two independent capabilities: appointment booking and document clearance.

The assignment states that a failure of the clearance service must not impede a citizen from being able to book an appointment. Each service must also manage its own data, without being able to access the data of the other service.

During Phase 1 of the implementation of Vulindlela, it must be able to run concurrently with the BranchDesk 2011 system. Vulindlela therefore needs to be developed to enable it to be added to the existing BranchDesk 2011 system.

# Decision

Vulindlela will be developed as two independent ASP.NET Core Web API services. The Booking Service will be developed to handle branches and appointment bookings. The Clearance Service will be developed to handle document clearances. Each service will be developed to have its own SQLite database.

The Booking Service will communicate to the Clearance Service via typed HttpClient.

The MVC portal will be developed to communicate to the two services, integrate their results and present them to citizens.

# Alternatives considered

Combining Booking and Clearance into one monolithic application was considered, but this was rejected because in the event of a failure to clear documents, appointment bookings could also be affected.

Services using a single database. Rejected because both services would depend on the same database structure. This creates a shared ownership of data and one service could become dependent on the other service’s internal implementation.

Separate services with separate databases. Selected because each service could manage its data and business logic independently, and communicate using defined HTTP APIs.

# Consequences

What we gain

The Clearance and Booking services can be independently developed, tested and maintained. A failure in one service does not impact the other.

What we accept

Communication using HTTP introduces additional points of failure and increases latency of requests. Independent databases make it difficult to coordinate business processes spanning more than one service. In the event one service is unavailable, the Portal must handle partial results and additional configuration and testing is needed to maintain communication between services.

Acceptance of these costs is warrantanted because of improved fault isolation and service independence.
