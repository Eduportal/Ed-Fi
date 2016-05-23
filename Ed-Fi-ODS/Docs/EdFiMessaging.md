EdFi.Messaging
==

EdFi has multiple options to facilitate messaging in EdFi software.  The persistent ODS uses messaging as part of the bulk processing to coordinate activities between the HTTP API and the services processing the uploaded files.  Please review the persistent ODS source code for a detailed example of EdFi messaging in action.  This document will introduce the concepts and features supported by the EdFi libraries.  

Messaging Concepts
==

##Message Delivery##
EdFi messaging is based on an 'at least once delivery' of messages.  This means the system will attempt to ensure messages are delivered at least once but may deliver them more than once as part of accomplishing this goal.  For this reason messages should be idempotent.  For a more detailed definition of idempotent messages please see this explanation from Jonathan Oliver: [http://blog.jonathanoliver.com/idempotency-patterns/](http://blog.jonathanoliver.com/idempotency-patterns/ "Idempotency patterns by Jonathan Oliver").

Under normal operating conditions messages will be delivery only once but in the event of system interruption or disaster recovery it may be necessary to 'replay' messages to ensure the state of the system is correct.  If the outage is recoverable by the system itself, it may deliver a previously delivered message as part of recovering services.  This is consistent with the behavior of most major messaging systems (Azure Service Bus, RabbitMQ, MQueueSeries, etc.).  Ensuring an only-once delivery would require distributed transactions and would cause significant performance penalties.

##Commands##
Commands are messages sent from a client to a server to execute some behavior.  Commands should be handled by a single handler at a time.  This does not preclude having more than one handler it simply means a distribution pattern should be employed (Competing Consumer is suggested).  Handler distribution is not fully supported by the EdFi Messaging libraries at this time.

##Events##
Events are messages about the result of executing behavior.  They represent a point in time or state at a given time and are naturally idempotent.  Events may be handled by as many handlers as are subscribed to them.  Events are not fully supported by the EdFi Messaging libraries at this time.

##Envelopes##
An envelope is a class that wraps messages and typically contains infrastructure data that is orthogonal to executing the behaviors inside of the message.  For example, security information can be embedded into an envelope and passed from endpoint to endpoint by the infrastructure to allow the executed behavior to run inside a specific security context.

##Handling Messages##
The software that ultimately processes messages is called a Message Handler.  A Message Handler should contain a method to receive the message, read its contents, and execute behavior based on those contents.  As mentioned above, EdFi Messaging based Handlers should contain idempotent behavior that will result in the same system state if a single message is delivered to a Handler more than once.  If the Handler's behavior will result in different state when a message is delivered more than once the n it represents a potential defect source and should be refactored to support idempotent behavior.  The following blog post presents a pattern for introducing idempotency when the Handler cannot be made natively idempotent: [http://lostechies.com/jimmybogard/2013/06/03/un-reliability-in-messaging-idempotency-and-de-duplication/](http://lostechies.com/jimmybogard/2013/06/03/un-reliability-in-messaging-idempotency-and-de-duplication/ "Idempotency Pattern by Jimmy Bogard").

##Message Endpoints##
Message Endpoints are used by the Clients to identify the source queue for their given message type.  What constitutes a Message Endpoint depends on the type of message being sent.  

For Commands a Message Endpoint is the location where the message should be sent to be handled.  This is usually a DNS name such as [Endpoint].[YourDomain].[COM | ORG | EDU | ETC] to allow the actual deployment of the Handler to be changed without altering the clients who send the Commands.  

Since events are not supported by EdFi Messaging this is the only definition that is used for this document.


Messaging Concepts in EdFi Messaging 
==

EdFi Messaging was created to support the messaging needs of Bulk File processing in the persistent ODS.  As such, it only contains the behaviors necessary for simple Command sending and receiving (handling).  Support for more advanced concepts such as distribution (multiple message handlers), Events (publish and subscribe), and RPC-like behaviors (request/reply).

##Creating a Command##
`EdFi.Common.Messaging` contains a marker interface `ICommand` to signify objects intended to be sent as commands from a client to a server.  Care should be taken when creating commands to ensure the execution of behavior based on a command is idempotent and will not result in invalid system state if delivered more than once.  See the EdFi ODS Bulk Processing implementation for an example of idempotent command behaviors or the example linked under the Handling Messages section above.


    public class ExampleCommand : ICommand
    {
    	public DateTime SentOn { get; set;}
    }
    

##Sending Commands##
`EdFi.Common.Messaging` has an `ICommandSender` interface and implementation to facilitate the sending of commands to a server (Message Endpoint).  Clients that needs to send a command should take a dependency on `ICommandSender` in the constructor and use the supplied instance at runtime to send a command using the `Send(ICommand)` method like the following example:

    public class ClientExample
    {
    	private ICommandSender sender;
    
    	public ClientExample(ICommandSender cmdSender)
    	{
    		sender = cmdSender;
    	}
    	
    	public void SendMessage()
    	{
    		var cmd = new ExampleCommand { SentOn = DateTime.UtcNow() };
    		sender.Send(cmd);
    	}
    } 

##Receiving Commands##
`EdFi.Common.Messaging` contains an `IMessageHandler<ICommand>` interface that should be implemented by a class you wish to have receive and process a Command message.  Handlers must create a method that accepts a message of the type declared in the class declaration.  For example:

    public class ExampleCommandHandler : IMessageHandler<ExampleCommand>
    {
    	public void Handle(ExampleCommand message)
    	{
    		Console.WriteLine("Recieved ExampleCommand message sent on {0}.", message.SentOn);
    	}
    }

##Using EdFi.Common.Messaging##
To use the messaging concepts in the EdFi libraries you must use `EdFiMessagingInstaller` to configure the container for messaging.  You must also use the installer for the version of messaging you wish to use.  The following example demonstrates how to register the core and MSMQ based implementations:

    public class RegisterMessagingForWindows
    {
    	public void Register(IWindsorContainer container)
    	{
    		IoCCommon.SupportCollections(container);  //necessary for sending and handling messages
    
    		IoC.RegisterTheWrappedServiceLocator(container);  //necessary for message handling only
    
    		container.Install(new EdFiCommonInstaller());  //necessary for sending and handling messages (reading the configuration and some other basic behaviors used by the implementations)
    
    		container.Install(new EdFiMessagingInstaller());  //This installs the EdFi common messaging behaviors
    
    		container.Install(new MSMQInstaller());  //This installs the MSMQ based messaging implementation for Windows

			container.AddFacility<LoggingFacility>(f => f.UseLog4Net());  //This causes Castle to supply the Log4Net logger for the Castle ILogger
    	}
    }

If you are only sending messages then the container must be configured to support collections.  You must also install the common EdFi behaviors.  These are used throughout the EdFi Messaging solutions.  If you are handling messages you must also register the container as a WrapperServiceLocator using `IoC.RegisterTheWrappedServiceLocator(IWindsorContainer)`.  Finally, you must register an implementation of the `Castle.Core.Logging.ILogger` - Castle has a Log4Net Facility that can be added as shown in the example above.

You also have the option of supplying a naming prefix to be prepended to the queue names in the configuration file.  Just add an appSetting with the key "QueuePrefix" and a value of whatever you wish - e.g. `<add key="QueuePrefix" value="YearSpecificSharedInstance" />`.  This allows you to run multiple queues for the same message type on a single server.  It can be useful when exploring or testing the various deployment options available in the ODS.  This feature is available as part of the core messaging and will be honored in any environment as long as you use the default implementation of the `IQueueNameProvider` interface.

#Messaging for Windows#

The `EdFi.Messaging.Windows` library contains the core implementations of common messaging necessary to use messaging in a Windows environment.  This library uses MSMQ which is available on any Windows environment.  You can install MSMQ in your environment by running the following powershell command from an Administrator PowerShell command prompt `Import-Module Servermanager` followed by `Add-WindowsFeature MSMQ` (see [this](http://technet.microsoft.com/en-us/library/cc731283%28v=ws.10%29.aspx "PowerShell MSMQ install") technet article for more details).  The first command loads the Server Manager cmdlets.  The second uses the Add-WindowsFeature cmdlet to enable the basic MSMQ features.  Only the basic MSMQ features are required for the EdFi solution.  You may also install MSMQ using the Windows Features GUI (see the [following](http://msdn.microsoft.com/en-us/library/vstudio/aa967729%28v=vs.110%29.aspx "Installing MSMQ via Windows Features GUI") MSDN article for more details).

To use the MSMQ library you must install the `MSMQInstaller` with your IoC container (see Using EdFi.Common.Messaging above).  If you are only sending messages you must edit your configuration file to include a named Message Endpoint value for each message type you will send.  The following example shows how to register these Endpoints:

    <appSettings>
    	<add key="ExampleCommandMessageEndpoint" value="localhost" />
    	<add key="AnotherCommandMessageEndpoint" value="handler.somewhere.com" />
    	<add key="YetAnotherCommandMessageEndpoint" value="." />
		<add key="QueuePrefix" value="sandbox" />
    </appSettings>

In the example above the `ExampleCommand` and `YetAnotherCommand` messages have their Endpoints on the local server that is sending the commands.  The [CommandName]MessageEndpoint pattern is used to identify a Messaging Endpoint configuration setting and the message type it specifies.  The value 'localhost' is equivalent to '.' but is preferred as it makes the configuration more readable.  The `AnotherCommand` message in the above example is configured to be at the 'handler.somewhere.com' location.  This DNS value 'handler.somewhere.com' must resolve to a MSMQ server location or a SystemException may be thrown explaining the endpoint as unreachable or non-existent.

If you are receiving (handling) messages then you may also wish to add the 'QueueAutoCreate' setting to the configuration to facilitate programmatic queue creation.  If you do not wish to allow the EdFi software to create queues automatically set the value to "1" (`<add key="QueueAutoCreate" value="1" />`).  If you do not wish to use programmatic queue creation then you must create the queues as transactional queues.  MSMQ Transactions are used by the MSMQ implementation to ensure at-least once delivery of messages.

#Messaging for Azure#

The `EdFi.Ods.Azure.Messaging` library contains the core implementations of common messaging necessary to use messaging in an Azure environment.  