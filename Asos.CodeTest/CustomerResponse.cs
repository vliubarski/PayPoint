namespace Asos.CodeTest
{
    public class CustomerResponse
    {
        public CustomerResponse()
        {
            Customer = new Customer();
        }

        public bool IsArchived { get; set; }

        public Customer Customer { get; set; }
    }
}