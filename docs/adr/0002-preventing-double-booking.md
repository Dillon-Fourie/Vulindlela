# ADR 0002: Enforce Appointment Slot Uniqueness at Database Level

# Context

Booking service users must be able to reserve appointments offered by various branches.

A concurrency problem happens when 2 or more users attempt to make the same booking at the same time. Application logic that checks booking availability before creating the booking may result in 2 or more bookings being made for the same slot.

Because the application logic cannot provide a guarantee to prevent 2 or more users from being booked into the same appointment, it is more reasonable to depend on a concurrency control mechanism that is enforced at the database level to provide such a guarantee.

# Decision

The Booking service will enforce a unique database index on the SlotId column of the Appointments table.

Entity Framework Core will configure the constraint as shown below:

modelBuilder.Entity()
    .HasIndex(appointment => appointment.SlotId)
    .IsUnique();

The application will check booking availability before creating the booking. The database unique constraint will, however, protect against duplicate bookings.

If 2 or more users attempt to book the same slot at the same time, the database constraint will prevent more than one appointment from being created. The application will capture and return a conflict response to the user attempting to make the duplicative booking.

# Alternatives considered

Application-level availability checking only. Rejected, because two requests processing concurrently can both check the slot before either of them inserts the appointment, thus believing the slot is available, and resulting in a race condition.

In-memory locking. Rejected because an in-memory lock only guarantees requests handled by the same application process. If the Booking service runs multiple instances, then each instance has its own lock and cannot prevent the other instance from booking the same slot.

Unique constraint on the database. Selected, because SQLite enforces unique constraints itself. Thus, the constraint ensures that an appointment can’t be booked for the same slot by more than one person, regardless of the number of requests reaching the service.

# Consequences

What we gain. 
The database ensures that an appointment slot cannot be assigned to more than one appointment. This protects against concurrent booking requests and enforces the rule independently of the application’s availability checks.

What we accept. 
The application has to recognize and take action for a violation of the database constraint. A citizen may get a ‘Conflict’ message when a different request is successful in booking the slot before theirs. The design also assumes that each appointment slot can only have one appointment. In the future if we need to support group bookings we would have to change the data model and concurrency strategy.

These consequences are acceptable, to ensure no duplicate appointments are made.