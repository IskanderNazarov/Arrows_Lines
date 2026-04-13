namespace Core._Purchasing {
    public class ProductInfo {
        //public string commonId;
        public string id;
        public string price; //key: price -->> value: 99 YAN
        public string priceValue; //key: priceValue -->> value: 99
        
        


        public override string ToString() {
            return $"Info:\n " +
                   //$"commonId: {commonId}\n" +
                   $"id: {id}\n" +
                   $"price: {price}\n" +
                   $"priceValue: {priceValue}\n"
                ;
        }
    }
}