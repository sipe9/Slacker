using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackerCmd
{
    public class Validator
    {
        public static bool HasIllegalPaths(Perforce.P4.Repository Repository, int ChangelistNumber, Configuration Config, out string Error)
        {
            Error = String.Empty;

            if (Repository == null)
            {
                Error = String.Format("[Slacker] Failed to check illegal paths from changelist because repository is null. Please check your P4 configurations and try again.");
                Console.WriteLine(Error);
                return false;
            }

            Perforce.P4.Changelist Changelist = null;

            try
            {
                Changelist = Repository.GetChangelist(ChangelistNumber);

                if (Changelist == null)
                {
                    Error = String.Format("[Slacker] Failed to search illegal paths from changelist because changelist {0} was not found from perforce repository.", ChangelistNumber);
                    Console.WriteLine(Error);
                    return false;
                }
            }
            catch (Perforce.P4.P4Exception ex)
            {
                Error = String.Format(ex.Message);
                Console.WriteLine(Error);
                return false;
            }

            foreach (var File in Changelist.Files)
            {
                var DepotPath = File.DepotPath.Path.Replace("\\", "/");

                if (!String.IsNullOrEmpty(DepotPath))
                {
                    var Extension = Path.GetExtension(DepotPath);
                    var DepotDirectory = Path.GetDirectoryName(DepotPath);

                    if (!String.IsNullOrEmpty(Extension) && !String.IsNullOrEmpty(DepotDirectory))
                    {
                        foreach (var Pair in Config.IllegalPaths)
                        {
                            if (Extension.ToLower() == Pair.Extension.ToLower() && DepotDirectory.ToLower().Contains(Pair.DepotDirectory.ToLower()))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsValidDescription(Perforce.P4.Repository Repository, int ChangelistNumber, Configuration Config, out string Error)
        {
            if (Repository == null)
            {
                Error = String.Format("[Slacker] Failed to build post submit string because repository is null. Please check your P4 configurations and try again.");
                Console.WriteLine(Error);
                return false;
            }

            Error = String.Empty;

            Perforce.P4.Changelist Changelist = null;

            try
            {
                Changelist = Repository.GetChangelist(ChangelistNumber);

                if (Changelist == null)
                {
                    Error = String.Format("[Slacker] Failed to check submit description becuase changelist because changelist {0} was not found from perforce repository.", ChangelistNumber);
                    Console.WriteLine(Error);
                    return false;
                }
            }
            catch (Perforce.P4.P4Exception ex)
            {
                Error = String.Format(ex.Message);
                Console.WriteLine(Error);
                return false;
            }
            
            var Description = Changelist.Description;

            // [Ticket-241][Reviewer: John Doe] Description here

            foreach (var Rule in Config.P4DescriptionRules)
            {   
            }

            return true;
        }
    }
}
