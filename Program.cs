using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace Program
{
    struct ClientInfo
    {
        public string Title;
        public string FirstName;
        public string MiddleName;
        public string LastName;

        public ClientInfo(string clientName)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            clientName = ti.ToTitleCase(clientName.ToLower());
            string[] words = clientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            this.Title = words.Length > 0 ? words[0] : "";
            this.FirstName = words.Length > 1 ? words[1] : "";
            this.LastName = words.Length > 2 ? words[words.Length - 1] : "";
            if (words.Length > 2)
                this.MiddleName = words.Length > 3 ? string.Join(" ", words.Skip(2).Take(words.Length - 3)) : "";
            else
                this.MiddleName = "";
        }
    };

    internal class Program
    {
        static (string addressee, string salutation) GenerateAddressAndSalutation(ClientInfo[] clients, Dictionary<string, string> pluralTitles)
        {
            int count = clients.Length;
            // if there are 2 clients
            if (count == 2)
            {
                if (clients[0].LastName.Equals(clients[1].LastName, StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].Title.Equals(clients[1].Title, StringComparison.OrdinalIgnoreCase))
                    {
                        // if same title, use plural
                        string plural = pluralTitles[clients[0].Title];
                        string add = $"{plural} {clients[0].FirstName.Substring(0, 1)} & {clients[1].FirstName.Substring(0, 1)} {clients[0].LastName}";
                        string sal = $"{plural} {clients[0].LastName}";
                        return (add, sal);
                    }
                    else
                    {
                        string add = $"{clients[0].Title} & {clients[1].Title} {clients[0].FirstName.Substring(0, 1)} & {clients[1].FirstName.Substring(0, 1)} {clients[0].LastName}";
                        string sal = $"{clients[0].Title} and {clients[1].Title} {clients[0].LastName}";
                        return (add, sal);
                    }
                }
                else
                {
                    // if different surnames, list both
                    string add = $"{clients[0].Title} {clients[0].FirstName.Substring(0, 1)} {clients[0].LastName} & {clients[1].Title} {clients[1].FirstName.Substring(0, 1)} {clients[1].LastName}";
                    string sal = $"{clients[0].Title} {clients[0].LastName} & {clients[1].Title} {clients[1].LastName},";
                    return (add, sal);
                }
            }
            else if (count == 3)
            {
                // when there are 3 clients
                // check if two clients have same surname
                var surnameGroups = clients.GroupBy(c => c.LastName).ToList();
                if (surnameGroups.Any(g => g.Count() == 2))
                {
                    var group = surnameGroups.First(g => g.Count() == 2);
                    string commonSurname = group.Key;
                    ClientInfo[] groupedClients = group.ToArray();
                    ClientInfo remainingClient = clients.First(c => !c.LastName.Equals(commonSurname, StringComparison.OrdinalIgnoreCase));

                    if (groupedClients[0].Title.Equals(groupedClients[1].Title, StringComparison.OrdinalIgnoreCase))
                    {
                        // if title is same use plural for grouped clients
                        string plural = pluralTitles[groupedClients[0].Title];
                        string add = $"{plural} {groupedClients[0].FirstName.Substring(0, 1)} & {groupedClients[1].FirstName.Substring(0, 1)} {commonSurname} & {remainingClient.Title} {remainingClient.FirstName.Substring(0, 1)} {remainingClient.LastName}";
                        string sal = $"{plural} {commonSurname} and {remainingClient.Title} {remainingClient.LastName},";
                        return (add, sal);
                    }
                    else
                    {
                        // Group the two with common surname using their titles.
                        string add = $"{groupedClients[0].Title} & {groupedClients[1].Title} {groupedClients[0].FirstName.Substring(0, 1)} & {groupedClients[1].FirstName.Substring(0, 1)} {commonSurname} & {remainingClient.Title} {remainingClient.FirstName.Substring(0, 1)} {remainingClient.LastName}";
                        string sal = $"{groupedClients[0].Title} and {groupedClients[1].Title} {commonSurname} and {remainingClient.Title} {remainingClient.LastName},";
                        return (add, sal);
                    }
                }
                else
                {
                    // if no common surname
                    // if all have same surname, use plural
                    if (clients.Select(c => c.Title).Distinct().Count() == 1)
                    {
                        string plural = pluralTitles[clients[0].Title];
                        string add = $"{plural} {clients[0].FirstName.Substring(0, 1)}, {clients[1].FirstName.Substring(0, 1)} & {clients[2].FirstName.Substring(0, 1)} {clients[0].LastName}";
                        string sal = $"{plural} {clients[0].LastName},";
                        return (add, sal);
                    }
                    else
                    {
                        // if all surnames are different, list all
                        string add = $"{clients[0].Title} {clients[0].FirstName.Substring(0, 1)} {clients[0].LastName}, {clients[1].Title} {clients[1].FirstName.Substring(0, 1)} {clients[1].LastName} & {clients[2].Title} {clients[2].FirstName.Substring(0, 1)} {clients[2].LastName}";
                        string sal = $"{clients[0].Title} {clients[0].LastName}, {clients[1].Title} {clients[1].LastName} & {clients[2].Title} {clients[2].LastName},";
                        return (add, sal);
                    }
                }
            }
            else
            {
                throw new Exception("Unsupported number of clients.");
            }
        }

        private static void Main(string[] args)
        {
            // Dictionary mapping recognized titles to plural forms.
            Dictionary<string, string> pluralTitles = new Dictionary<string, string>
            {
                { "Mr", "Messrs" },
                { "Mrs", "Mesdames" },
                { "Ms", "Mses" },
                { "Miss", "Misses" }
            };

            // Helper: run test case and display results.
            void RunTestCase(string testCaseLabel, ClientInfo[] clients)
            {
                // Validate titles.
                foreach (ClientInfo c in clients)
                {
                    if (!pluralTitles.ContainsKey(c.Title))
                        throw new Exception("Unknown Title: " + c.Title);
                }
                var (addressee, salutation) = GenerateAddressAndSalutation(clients, pluralTitles);
                Console.WriteLine($"--- {testCaseLabel} ---");
                Console.WriteLine("Addressee: " + addressee);
                Console.WriteLine("Salutation: " + salutation);
                Console.WriteLine();
            }

            // Test Cases:

            // Example 1 (2 clients, distinct titles but same surname)
            RunTestCase("Example 1",
                new ClientInfo[] {
                    new ClientInfo("Mr Michael Paul Sargent"),
                    new ClientInfo("Mrs Victoria Sargent")
                });

            // Example 2 (2 clients, same title and same surname)
            RunTestCase("Example 2",
                new ClientInfo[] {
                    new ClientInfo("Mrs Michelle Clifford"),
                    new ClientInfo("Mrs Bethany Clifford")
                });

            // Example 3 (2 clients, same title and same surname)
            RunTestCase("Example 3",
                new ClientInfo[] {
                    new ClientInfo("Mr Yashie Clifford"),
                    new ClientInfo("Mr Varun Clifford")
                });

            // Example 4 (2 clients, same title and same surname)
            RunTestCase("Example 4",
                new ClientInfo[] {
                    new ClientInfo("Ms Mei Yuan"),
                    new ClientInfo("Ms Simone Yuan")
                });

            // Example 5 (2 clients, same title and same surname)
            RunTestCase("Example 5",
                new ClientInfo[] {
                    new ClientInfo("Miss Mei Yuan"),
                    new ClientInfo("Miss Simone Yuan")
                });

            // Example 6 (3 clients: two with same surname, grouped; one separate)
            RunTestCase("Example 6",
                new ClientInfo[] {
                    new ClientInfo("Mr Yashie Clifford"),
                    new ClientInfo("Mr Varun Clifford"),
                    new ClientInfo("Miss Simone Yuan")
                });
        }
    }
}

