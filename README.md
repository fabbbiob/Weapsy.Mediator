# Weapsy.Mediator

[![Build status](https://ci.appveyor.com/api/projects/status/p5p80y0fa6e9wbaa?svg=true)](https://ci.appveyor.com/project/lucabriguglia/weapsy-mediator)

Mediator for .NET Core that can be used in many scenarios, from a simple command/query pattern to a more complex CQRS with Event Sourcing implementation.

## Installing Weapsy.Mediator

Nuget Packages

[![Nuget Package](https://img.shields.io/badge/Weapsy.Mediator-1.4.0-brightgreen.svg)](https://www.nuget.org/packages/Weapsy.Mediator)
[![Nuget Package](https://img.shields.io/badge/Weapsy.Mediator.EventStore.EF-1.4.0-brightgreen.svg)](https://www.nuget.org/packages/Weapsy.Mediator.EventStore.EF)

Via Package Manager

    Install-Package Weapsy.Mediator
    
Or via .NET CLI

    dotnet add package Weapsy.Mediator
    
Or via Paket CLI

    paket add Weapsy.Mediator

For Event Sourcing, an event store provider needs to be installed.
At the moment, the only available is the entity framework event store but more will be added soon (Blob Storage, Xml and MongoDB).

Via Package Manager

    Install-Package Weapsy.Mediator.EventStore.EF
    
Or via .NET CLI

    dotnet add package Weapsy.Mediator.EventStore.EF
    
Or via Paket CLI

    paket add Weapsy.Mediator.EventStore.EF

## Using Weapsy.Mediator

A fully working example, including CQRS and Event Sourcing, is available in the examples folder of the repository https://github.com/Weapsy/Weapsy.Mediator/tree/master/examples

### Register services

In ConfigureServices method of Startup.cs:

```C#
services.AddWeapsyMediator(typeof(CreateProduct), typeof(GetProduct));
```

CreateProduct is an sample command and GetProduct is a sample query.
In this scenario, commands and queries are in two different assemblies.
Both assemblies need to be registered.
In order to use the event sourcing functionalities, an event store provider needs to be added as well.
In this case, we are using the entity framework event store.

```C#
services.AddWeapsyMediatorEF(Configuration);
```

The EFEventStore uses a db context and it needs to be configured.
First, add the following options in appsettings.json :

```JSON
"MediatorData": {
	"Type": "EF", // Options: EF, XML, BlobStorage
	"EFProvider": "MSSQL" // Options: MSSQL, MySQL, SQLite, PostgreSQL
},
"ConnectionStrings": {
	"MediatorConnection": "Server=(localdb)\\mssqllocaldb;Database=EventStore;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
Next, configure the options when registering the services:

```C#
services.AddOptions();
services.AddWeapsyMediatorEFOptions(Configuration);
```

If entity framework is used, databse can be installed adding this line in the Configure method of Startup.cs:

```C#
mediatorDbContext.Database.Migrate();
```

mediatorDbContext is passed as a parameter:

```C#
public void Configure(IApplicationBuilder app, IHostingEnvironment env, MediatorDbContext mediatorDbContext)
```

### Basics

Note that all handlers are available as asynchronous and as well as synchronous, but for these examples I'm using the asynchronous versions only.

There are 3 kinds of messages:
- Command, single handler
- Event, multiple handlers
- Query/Result, single handler that returns a result

#### Command (simple usage)

First, create a message:

```C#
public class DoSomething : ICommand
{
}
```

Next, create the handler:

```C#
public class DoSomethingHandlerAsync : ICommandHandlerAsync<DoSomething>
{
    public Task HandleAsync(DoSomething command)
    {
        await _myService.MyMethodAsync();
    }
}
```

And finally, send the command using the mediator:

```C#
var command = new DoSomething();
await _mediator.SendAsync(command)
```

#### Command (with events)

Using the SendAndPublishAsync method, the mediator will automatically publish the events returned by the handler.

First, create a command and an event:

```C#
public class DoSomething : ICommand
{
}

public class SomethingHappened : IEvent
{
}
```

Next, create the handler:

```C#
public class DoSomethingHandlerAsync : ICommandHandlerWithEventsAsync<DoSomething>
{
    public Task<IEnumerable<IEvent>> HandleAsync(DoSomething command)
    {
        await _myService.MyMethodAsync();
        return new List<IEvent>{new SomethingHappened()};
    }
}
```

And finally, send the command and publish the events using the mediator:

```C#
var command = new DoSomething();
await _mediator.SendAndPublishAsync(command)
```

#### Event

First, create a message:

```C#
public class SomethingHappened : IEvent
{
}
```

Next, create one or more handlers:

```C#
public class SomethingHappenedHandlerAsyncOne : IEventHandlerAsync<SomethingHappened>
{
    public Task HandleAsync(SomethingHappened @event)
    {
        await _myService.MyMethodAsync();
    }
}

public class SomethingHappenedHandlerAsyncTwo : IEventHandlerAsync<SomethingHappened>
{
    public Task HandleAsync(SomethingHappened @event)
    {
        await _myService.MyMethodAsync();
    }
}
```

And finally, publish the event using the mediator:

```C#
var @event = new SomethingHappened();
await _mediator.PublishAsync(@event)
```

#### Query/Result

First, create a model and a message:

```C#
public class Something
{
    public int Id { get; set; }
}

public class GetSomething : ICommand
{
    public int Id { get; set; }
}
```

Next, create the handler:

```C#
public class GetSomethingQueryHandlerAsync : IQueryHandlerAsync<GetSomething, Something>
{
    public async Task<Something> RetrieveAsync(GetSomething query)
    {
        return await _db.Somethings.FirstOrDefaultAsync(x => x.Id == query.Id);
    }
}
```

And finally, get the result using the mediator:

```C#
var query = new GetSomething{ Id = 123 };
var something = await _mediator.GetResultAsync<GetSomething, Something>(query);
```

### Advanced (CQRS and Event Sourcing)

Using the SendAndPublishAsync<IDomainCommand, IAggregateRoot> method, the mediator will automatically publish the events returned by the handler and save those events in the event store.
A working example can be found at https://github.com/Weapsy/Weapsy.Mediator/tree/master/examples

First, create a command and an event:

```C#
public class CreateProduct : DomainCommand
{
    public string Title { get; set; }
}

public class ProductCreated : DomainEvent
{
    public string Title { get; set; }
}
```

Next, create a domain object that inherits from AggregateRoot.
This is how a Procuct class might look like:

```C#
public class Product : AggregateRoot
{
    public string Title { get; private set; }

    public Product()
    {            
    }

    public Product(Guid id, string title) : base(id)
    {
        if (string.IsNullOrEmpty(title))
            throw new ApplicationException("Product title is required.");

        AddEvent(new ProductCreated
        {
            AggregateId = Id,
            Title = title
        });
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new ApplicationException("Product title is required.");

        AddEvent(new ProductTitleUpdated
        {
            AggregateId = Id,
            Title = title
        });
    }

    public void Apply(ProductCreated @event)
    {
        Id = @event.AggregateId;
        Title = @event.Title;
    }

    public void Apply(ProductTitleUpdated @event)
    {
        Title = @event.Title;
    }
}
```

Note that the empty constructor is required in order to create a new object using reflection.
After every command has been executed, an event is added to the pending event list calling the AddEvent method.
The Apply methods are called automatically when new events are added and are also used to load the object from the history when GetById method of the Repository is called.
Create the first handler:

```C#
public class CreateProductHandlerAsync : IDomainCommandHandlerAsync<CreateProduct>
{
    public async Task<IEnumerable<IDomainEvent>> HandleAsync(CreateProduct command)
    {
        await Task.CompletedTask;

        var product = new Product(command.AggregateId, command.Title);

        return product.Events;
    }
}
```

Send the command using the mediator:

```C#
var command = new CreateProduct
{
    AggregateId = Guid.NewGuid(),
    Title = "My brand new product"
};
await _mediator.SendAndPublishAsync<CreateProduct, Product>(command)
```

In a CQRS scenario, we want to create our read model.
It can be achieved by creating an event handler:

```C#
public class ProductCreatedHandlerAsync : IEventHandlerAsync<ProductCreated>
{
    public async Task HandleAsync(ProductCreated @event)
    {
        await Task.CompletedTask;

        var model = new ProductViewModel
        {
            Id = @event.AggregateId,
            Title = @event.Title
        };

        FakeReadDatabase.Products.Add(model);
    }
}
```

At this point, the aggregate and the first event have been saved in the event store and the product can be retrieved from the repository.
New commands, events and handlers can now be added:

```C#
public class UpdateProductTitle : DomainCommand
{
    public string Title { get; set; }
}

public class ProductTitleUpdated : DomainEvent
{
    public string Title { get; set; }
}

public class UpdateProductTitleHandlerAsync : IDomainCommandHandlerAsync<UpdateProductTitle>
{
    private readonly IRepository<Product> _repository;

    public UpdateProductTitleHandlerAsync(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IDomainEvent>> HandleAsync(UpdateProductTitle command)
    {
        var product = await _repository.GetByIdAsync(command.AggregateId);

        if (product == null)
            throw new ApplicationException("Product not found.");

        product.UpdateTitle(command.Title);

        return product.Events;
    }
}

public class ProductTitleUpdatedHandlerAsync : IEventHandlerAsync<ProductTitleUpdated>
{
    public async Task HandleAsync(ProductTitleUpdated @event)
    {
        await Task.CompletedTask;

        var model = FakeReadDatabase.Products.Find(x => x.Id == @event.AggregateId);
        model.Title = @event.Title;
    }
}
```

As per prevoius example, the mediator can be used to update the product.

```C#
await mediator.SendAndPublishAsync<UpdateProductTitle, Product>(new UpdateProductTitle
{
    AggregateId = productId,
    Title = "Updated product title"
});
```

A new event is saved and the read model is updated using the event handler.
Next time the aggregate is loaded from the repository, two events will be applied in order to recreate the current state.

## Roadmap

- Add more event store providers
    - Xml
    - Blob Storage
    - MongoDB
- Add custom domain event properties
