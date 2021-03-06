// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using FluentSharp.CoreLib.API;
using FluentSharp.CoreLib.Interfaces;
using O2.Views.ASCX.MerlinWizard;
using O2.Views.ASCX.MerlinWizard.O2Wizard_ExtensionMethods;
using Merlin;

//O2Ref:merlin.dll
//O2Ref:System.Management.dll

//O2File:O2Wizard.cs

namespace O2.Script
{
    public class Wizard_EnabledDisable_NetworkAdapters
    {    
    	private static IO2Log log = PublicDI.log;

        public void runWizard()
        {
        	var o2Wizard = new O2Wizard("Enable/Disable Network Adapters", 500, 500);		
        	if (false == Netsh.isAvailable())
        		o2Wizard.Steps.add_Message("Error onEnable/Disable Network Adapters","no NetSh found on this system");
        	else
        	{        	
        		// step 1
        		var step1Message = string.Format("This wizard will allow the enable or disable the following Windows Network adapters: \r\n {0}\r\n{1}", 
        									 	Netsh.interfaces_getDetails(),
        									 	Netsh.interfaces_getIPConfig());
        		o2Wizard.Steps.add_Message("Current Network Adapter details", step1Message);        		        		
        		// step 2
        		o2Wizard.Steps.add_Action("Disabling Network Adapters", disableNetworkAdapters);
        		// step 3
        		o2Wizard.Steps.add_Action("Ping www.google.com", (step) => pingAddress(step,"www.google.com"));
        		// step 4
        		o2Wizard.Steps.add_Action("Enabling Network Adapters", enableNetworkAdapters);        		
        		// step 5
        		o2Wizard.Steps.add_Action("Ping www.google.com", (step) => pingAddress(step,"www.google.com"));
        		// step 6
        		o2Wizard.Steps.add_Action("All Done", endOfWizard);
        	}        	
        	o2Wizard.start();
        }    	    	    	    	    
        
        public void disableNetworkAdapters(IStep step)
        {
        	networkAdapterAction(step,"Disabling", Netsh.interface_Disable);
        }
        
        public void enableNetworkAdapters(IStep step)
        {
        	networkAdapterAction(step,"Enabling", Netsh.interface_Enable);
        }
        
        public void networkAdapterAction(IStep step, string actionName,Func<string,string> methodToExecute)
        {
        	O2Thread.mtaThread(
        		()=> {
        				step.allowNext(false);
        				step.append_Line(actionName + " Network Adapter(s)", true /*extraLineAfter */, true /*extraLineBefore */);        				
			        	var networkAdapters = Netsh.interfaces_getNames();
			        	foreach(var networkAdapter in networkAdapters)
			        	{
			        		var result = methodToExecute(networkAdapter).Trim();
			        		if (result == "")
			        			step.append_Line(" - " + networkAdapter);			        		
			        	}			        	
						step.append_Line("step completed",false /*extraLineAfter */ ,true /*extraLineBefore */);
						step.allowNext(true);
					});
        }
        
        public void pingAddress(IStep step, string pingTarget)
        {
        	O2Thread.mtaThread(
        		()=> {
        			step.allowNext(false);        				
        				step.append_Line("Sending a ping request to: " + pingTarget,true,true);
        				var processName = "ping.exe";
        				var processParams = pingTarget;
        				var process = Processes.startProcessAsConsoleApplication(processName,processParams, 
        					(sender, e) => processDataReceived(step, e.Data)
        				 	,null);								
						process.WaitForExit();
						step.append_Line("step completed");
						step.allowNext(true);
					 });        			
        }
        
        public void processDataReceived(IStep step, string dataReceived) 
        {
	    	if (dataReceived != "")	    	
	        	step.append_Line(" {0}", dataReceived);					            
		}
		
		public void endOfWizard(IStep step)
		{
			step.set_Text("End of Wizard");
			step.allowBack(true);
			step.allowCancel(false);
		}
    }
    
    
}


