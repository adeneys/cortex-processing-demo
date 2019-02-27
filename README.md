# Sitecore Cortex Processing Engine Demo #

This repository shows how to extend the Sitecore Cortex Processing Engine. It includes:

* Custom tasks
* Integration with external services
* Registering task chains from an external application
* Retreiving the status of a task

This repository accompanies the videos from the Sitecore Cortex Processing Engine series:

* (Sitecore Cortex - Processing Engine Architecture)[https://www.youtube.com/watch?v=gNUu0QuW09g]
* (Sitecore Cortex - Processing Engine Tasks)[https://www.youtube.com/watch?v=DbDSwirB4Oc]

# How to Build #

* Set the URI for your xConnect server inside the `settings.xml` file in the console project. The config path is `/settings/xconnect/uri`
* WARNING: This demo doesn't support sending client certificates for the xConnect server, so ensure client certificate verification has been disabled on the xConnect server.
* Set the connection string for the message buses inside the `settings.xml` file in the console project. This must be the same database as the procesing engine uses for the message bus (messaging connection string). The config paths are:
** `/settings/rebus/Sitecore.Processing.Tasks.Messaging.Buses.TaskRegistrationProducer/Transport/SqlServer/ConnectionStringOrName`
** `/settings/rebus/Sitecore.Processing.Tasks.Messaging.Buses.TaskProgressProducer/Transport/SqlServer/ConnectionStringOrName`
* Build the solution using Visual Studio

# How to Deploy #

## Deploy the xConnect model to the xConnect server ##
* Run the console project and execute step 'a' to serialize the model to JSON
* Copy the JSON file (output in the consoles `bin` folder) to the `\Add_Data\Models` folder on the xConnect server
* Restart the xConnect server

## Deploy the xConnect model to the Cortex Processing Engine ##
* Copy the DLL files from the output folder of the xConnect extensions project to the Processing Engine folder

## Deploy the Processing Engine extensions to the Processing Engine ##
* Copy the DLL files from the output folder of the Processing Engine extensions project to the Processing Engine folder
* Copy the `App_Data` folder from the Processing Engine extensions project to the Processing Engine
* Set the Spotify ClientID and ClientSecret settings in the `\App_Data\Config\Sitecore\Demo\sc.Spotify.xml` file of the Processing Engine
