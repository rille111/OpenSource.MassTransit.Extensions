# Description
Infrastructure project contains boilerplate, conventions and helper methods to make usage of MassTransit a breeze to use.
Some stuff isn't implemented yet so feel free implementing it. For example, extra configuration for message TTL etc.

## Step by step usage
* Add Messaging.Infrastructure.ServiceBus
* Nuget add pkgs: MassTransit (and all dependencies) and optional: MassTransit.SimpleInjector, and consolidate packages in solution.
* In your IoC/Bootup class:
	* Register BusConfiguration (as a factory method, connection to the service bus)
		* You can find the connection info in the Azure portal: Bus - Shared Access Policies - click the Policy - observe connection string
	* Register IBusConfigurator (choose AzureSbBusConfigurator or RabbitMqBusConfigurator)
	* Register IBusControl (as a factory method, IBusConfigurator will give you one. This is the actual Bus that you use to send, publish, receive)
	* Upon program startup, Instantiate the IBusControl and call .Start()
	* Don't forget to .Stop(), so that client-specific queues get deleted
* More Code & usage: see example projects (Messaging.Lab.Sender.frmMain.cs and Messaging.Lab.Receiver.Program.cs)

IMPORTANT
* Always use IBusControl, not IBus!
* For messages (separate Commands and Events) always use interface as a base, implement them, and send/publish using the classes not the interfaces, for type safety!!!
	* Ex: IDominateCountryCommand  (interface) > DominateSwedenCommand (class)
	* Ex: ICountryDominatedEvent   (interface) > SwedenDominatedEvent : ICountryDominatedEvent
	* Ex: ICountryDominatedEvent   (interface) > SwedenDominatedEvent : ICountryDominatedEvent, ICountryDominatedVersionTwo
* You should always send to exactly one send endpoint (queue) for every message's super-type (the inteface, ie ICountryDominatedEvent, IDominateCountryCommmand), and the sub-types (classes) will share that same topic/queue.
	Example: bus.GetSendEndpointAsync<IDominateCountryCommand>(); and bus.Send(new DominateSwedenCommand)
	* Again: in the same manner, you should cfg.ReceiveEndpoint only on the super-type (interface)
* Use Azure Service Bus Explorer for investigating the queues, just paste the connection string

## Setup with submodule
* Add submodule > git submodule add git@git:Common/Messaging.git [foldername]
* Open the solutionfile for this project (Messaging.Infrastructure.sln)
* Rebuild, so that packages are restored
* Open your root solution
* Add this project and rebuild
* Git commit now!

## Conventions & Patterns
* Commands  Naming: [Verb][Noun]Command ie 'UpdateOrderCommand' - should be sent by using Bus.Send(), or Endpoint.Send() - only to be executed once, single service instance performs the command action. 
* Queries   Naming: Get[Noun]Query ie 'GetOrderQuery' - should be sent by using Bus.Send(), or Endpoint.Send(), with request/reply pattern always expecting an answer. Use these as you would use Commands.
* Events    Naming: [Noun][Verb]Event ie 'OrderUpdatedEvent' - should be published by using Bus.Publish() - to be observed by multiple consumers, see https://www.maldworth.com/2015/10/27/masstransit-send-vs-publish/

## Queue names and setup
Use the top-most type, the interface - as the name of the Topic/Queue.
See: http://docs.masstransit-project.com/en/v2-develop/overview/versioning.html and https://github.com/MassTransit/MassTransit/blob/develop/doc/source/usage/messages.rst

## Usage examples
* Configuration:
	```
        var busConfig = new BusConfiguration
        {
            
            // Endpoint=sb://......servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc
            ConnectionUri = "sb://.....servicebus.windows.net", 
            Login = "RootManageSharedAccessKey",
            Password = "abc"
        };
        return busConfig;
	```
* When not using IoC:
	See the lab projects (fromMain.cs and Program.cs)

	```
		IBusFactoryConfigurator cfg;

        // Command Consumers 
        cfg.ReceiveEndpoint<IDominateCountryCommand>(c =>
        {
            // Since we share one endpoint, both command consumers will get the message.
            c.Consumer<DominateCountryConsumer>();
            c.Consumer<DominateSwedenConsumer>();
        });

        // Event Consumers
        cfg.ReceiveEndpoint<ICountryWasDominatedEvent>(c =>
        {
            // Since we share one endpoint, both command consumers will get the message.
            c.Consumer<CountryDominatedConsumer>();
            c.Consumer<SwedenDominatedConsumer>();
        });

		cfg.UseRetry(Retry.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5)));
	```
		
* When using IoC. Example:
	```
		container.Register<BusConfiguration>(() => config);
		container.Register<HelloWorldConsumer>(); // Not sure if this is necessary because the SimpleInjectorMassTransit-pkg may be scanning for everything implementing IConsumer<>.
        public static Container CreateCommonContainer(string[] withArguments)
        {
            // 1. Create a new Simple Injector container
            var container = new Container();

            // 2. Configure the container (register)
            container.Register<IBusControl>(() => CreateIBusControl(container));
            return container;
        }
        private static IBusControl CreateIBusControl(Container withContainer)
        {
            var configurator = withContainer.GetInstance<IBusConfigurator>();
            return configurator
                .CreateBus((cfg, host) => 
				{
					cfg.ReceiveEndpoint<IDominateCountryCommand> c => c.LoadFrom(withContainer)))
					cfg.UseRetry(Retry.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5)));
				}
               
        }	
	```

## Help & Refs
http://docs.masstransit-project.com/en/latest/usage/consumer.html
http://docs.masstransit-project.com/en/latest/usage/messages.html -. on commands vs events, naming and semantics

==== Examples of adding Consumers (inside the factory) ====
// Creating object (parameterless constructor)
e.Consumer(consumer, type => Activator.CreateInstance(consumer));

// connected using the simplest method, which accepts a consumer class with a default constructor.
e.Consumer<UpdateCustomerConsumer>();

// Using an IoC container
e.LoadFrom(_container);

// an anonymous factory method
e.Consumer(() => new YourConsumer());

// an existing consumer factory for the consumer type
e.Consumer(consumerFactory);

// a type-based factory that returns an object (container friendly)
e.Consumer(consumer, type => container.Resolve(type));
e.Consumer(consumer, type => Activator.CreateInstance(consumer));

// an anonymous factory method, with some middleware goodness
e.Consumer(() => new YourConsumer(), x =>
{
    // add middleware to the consumer pipeline
    x.UseLog(ConsoleOut, async context => "Consumer created");
});

