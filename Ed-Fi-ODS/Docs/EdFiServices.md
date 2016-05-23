#EdFi Services

There are three main components provided by the core EdFi libraries to facilitate running services in a Windows or Windows Azure environment.  There is a core interface in EdFi.Common, a tools library for Windows services and a base class for Azure worker roles.  Choosing which environment to support is beyond the scope of this document.

EdFi.Common.Services
==
EdFi.Common.Services has the `IHostedService` interface that should be implemented by any library containing software to run as a service.

    namespace EdFi.Common.Services
    {
	    public interface IHostedService
	    {
	    void Start();
	    void Stop();
	    
	    /// <summary>
	    /// Provides the description to be displayed in Windows Services Console or other management programs
	    /// </summary>
	    string Description { get; }
	    
	    /// <summary>
	    /// Provides the name to be displayed in Windows Services Console or other managemenet programs
	    /// </summary>
	    string DisplayName { get; }
	    
	    /// <summary>
	    /// Provides the name to be shown in the Windows Services Console as the Service Name - may be the same or different from display name
	    /// </summary>
	    string ServiceName { get; }
	    }
    }
    

The `Start()` method should contain the code to be executed once the service is running.  If you are hosting a message based endpoint any listener would be started in this method.
The `Stop()` method should contain the code to be executed when the service is being shut down.  This should include disposing of any resources and closing any connections.

EdFi.Services.Windows
==
If your software is intended to be hosted in a Windows environment then the EdFi.Services.Windows library provides several tools to facilitate the creation, debugging, installation, and management of software as Windows Services.

This is primarily accomplished via TopShelf ().  TopShelf simplifies the creation and installation of Windows services through `Topshelf.Host`.  This wrapper takes care of satisfying the Windows requirements for services and is the hook into those services.  

The EdFi.Services.Windows library simplifies the creating and using of the `Topshelf.Host` object.  The `WindowsServiceInitiator` class has a static `InitiateService` method that takes a `Func<IHostedService>` and returns a `Host` object configured for that `IHostedService`.

To create a Windows service using these tools:

1.  Create an `IHostedService`.  

1.  Create a console application.  

1.  In the `Main` method body use the `WindowsServiceInitiator.InitiateService` method to create a Host instance based on your implementation.  Calling the `Host.Run()` method of the returned Host object starts the service.

Here's an example:

    namespace EdFi.Services.Example
    {
	    class Program
	    {
		    static void Main(string[] args)
		    {
			    ConfigureIoC();

			    // this returns a Topshelf.Host instance based on the ExampleHostedService
			    var host = WindowsServiceInitiator.InitiateService(IoC.Resolve<IHostedService>);
			    			
			    // this starts the service
			    host.Run();
		    }
		    
		    private static void ConfigureIoC()
		    {
		    	// this should include all the registration necessary to support the supplied IHostedService container
		    	container.Register(Component.For<IHostedService>().ImplementedBy<ExampleHostedService>());
		    }
	    }
    }

To debug a service created with these tools, open the source code in Visual Studio and start the console application in debug mode.

To install a service created with these tools into a Windows environment as a service that can be started, stopped, and restarted open a command line prompt in Adminstrator mode and execute the [YourServiceConsoleAppName].exe produced by compiling your console applicaiton using the 'install' flag.  For the service above this would look like `c:\downloads\ExampleHostedService.exe install`.  This method can also be used to uninstall the service by replacing the 'install' flag with 'uninstall'.  See [http://topshelf.readthedocs.org/en/latest/overview/commandline.html](http://topshelf.readthedocs.org/en/latest/overview/commandline.html "Topshelf Command Line Reference") for more details.


EdFi.Ods.WindowsAzure.Services
==
If your software is instended to be hosted in an Azure environment then the EdFi.Ods.WindowsAzure library provides a `WorkerRoleBase<IHostedService>` object that should be the parent of any worker role objects you create.