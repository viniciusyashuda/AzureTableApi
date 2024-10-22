using Azure.Data.Tables;
using AzureTableApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureTableApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public ContactController(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("SAConnectionString");
            _tableName = configuration.GetValue<string>("AzureTableName");
        }

        private TableClient GetTableClient()
        {
            var serviceClient = new TableServiceClient(_connectionString);
            var tableClient = serviceClient.GetTableClient(_tableName);

            tableClient.CreateIfNotExists();

            return tableClient;
        }

        [HttpPost]
        public IActionResult Create(Contact contact)
        {
            var table = GetTableClient();

            contact.RowKey = Guid.NewGuid().ToString();
            contact.PartitionKey = contact.RowKey;

            table.UpsertEntity(contact);

            return Ok(contact);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Contact updatedContact, string id)
        {
            var table = GetTableClient();
            var contact = table.GetEntity<Contact>(id, id).Value;

            contact.Name = updatedContact.Name;
            contact.PhoneNumber = updatedContact.PhoneNumber;
            contact.Email = updatedContact.Email;

            table.UpsertEntity(contact);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var table = GetTableClient();
            var contacts = table.Query<Contact>().ToList();

            return Ok(contacts);
        }

        [HttpGet("{name}")]
        public IActionResult GetByName(string name)
        {
            var table = GetTableClient();
            var contacts = table.Query<Contact>(contact => 
                contact.Name.ToLower() == name.ToLower()
            ).ToList();

            return Ok(contacts);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var table = GetTableClient();
            table.DeleteEntity(id, id);

            return NoContent();
        }
    }
}