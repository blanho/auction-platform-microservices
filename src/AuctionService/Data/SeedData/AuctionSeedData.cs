using AuctionService.Entities;

namespace AuctionService.Data.SeedData
{

    public static class AuctionSeedData
    {
        public static List<Auction> GetAuctions()
        {
            return new List<Auction>
            {
                CreateLiveAuction(
                    id: "afbee524-5972-4075-8800-7d1f9d7b0a0c",
                    seller: "bob",
                    reservePrice: 20000,
                    daysUntilEnd: 10,
                    make: "Ford",
                    model: "GT",
                    color: "White",
                    mileage: 50000,
                    year: 2020,
                    imageUrl: "https://cdn.pixabay.com/photo/2016/05/06/16/32/car-1376190_960_720.jpg"
                ),

                CreateLiveAuction(
                    id: "c8c3ec17-01bf-49db-82aa-1ef80b833a9f",
                    seller: "alice",
                    reservePrice: 90000,
                    daysUntilEnd: 60,
                    make: "Bugatti",
                    model: "Veyron",
                    color: "Black",
                    mileage: 15035,
                    year: 2018,
                    imageUrl: "https://cdn.pixabay.com/photo/2012/05/29/00/43/car-49278_960_720.jpg"
                ),

                CreateFinishedAuction(
                    id: "bbab4d5a-8565-48b1-9450-5ac2a5c4a654",
                    seller: "bob",
                    winner: "alice",
                    soldAmount: 23000,
                    daysAgoEnded: 10,
                    make: "Ford",
                    model: "Mustang",
                    color: "Red",
                    mileage: 65125,
                    year: 2023,
                    imageUrl: "https://cdn.pixabay.com/photo/2012/11/02/13/02/car-63930_960_720.jpg"
                ),

                CreateLiveAuction(
                    id: "155225c1-4448-4066-9886-6786536e05ea",
                    seller: "tom",
                    reservePrice: 50000,
                    daysUntilEnd: 30,
                    make: "Mercedes",
                    model: "SLK",
                    color: "Silver",
                    mileage: 15001,
                    year: 2020,
                    imageUrl: "https://cdn.pixabay.com/photo/2016/04/17/22/10/mercedes-benz-1335674_960_720.png"
                ),

                CreateReserveNotMetAuction(
                    id: "466e4744-4dc5-4987-aae0-b621acfc5e39",
                    seller: "alice",
                    reservePrice: 20000,
                    currentHighBid: 18000,
                    daysAgoEnded: 5,
                    make: "BMW",
                    model: "X1",
                    color: "White",
                    mileage: 90000,
                    year: 2017,
                    imageUrl: "https://cdn.pixabay.com/photo/2017/08/31/05/47/bmw-2699538_960_720.jpg"
                ),

                CreateLiveAuction(
                    id: "dc1e4071-d19d-459b-b848-b5c3cd3d151f",
                    seller: "tom",
                    reservePrice: 150000,
                    daysUntilEnd: 21,
                    make: "Ferrari",
                    model: "Spider",
                    color: "Red",
                    mileage: 50000,
                    year: 2015,
                    imageUrl: "https://cdn.pixabay.com/photo/2017/11/09/01/49/ferrari-458-spider-2932191_960_720.jpg"
                ),

                CreateLiveAuction(
                    id: "47111973-d176-4feb-848d-0ea22641c31a",
                    seller: "bob",
                    reservePrice: 95000,
                    daysUntilEnd: 45,
                    make: "Ferrari",
                    model: "F-430",
                    color: "Red",
                    mileage: 5000,
                    year: 2022,
                    imageUrl: "https://cdn.pixabay.com/photo/2017/11/08/14/39/ferrari-f430-2930661_960_720.jpg"
                ),

                CreateLiveAuction(
                    id: "6a5011a1-fe1f-47df-9a32-b5346b289391",
                    seller: "alice",
                    reservePrice: 0,
                    daysUntilEnd: 19,
                    make: "Audi",
                    model: "R8",
                    color: "White",
                    mileage: 10050,
                    year: 2021,
                    imageUrl: "https://cdn.pixabay.com/photo/2019/12/26/20/50/audi-r8-4721217_960_720.jpg"
                ),

                CreateFinishedAuction(
                    id: "40490065-dac7-46b6-acc4-df507e0d6570",
                    seller: "tom",
                    winner: "bob",
                    soldAmount: 28000,
                    daysAgoEnded: 20,
                    make: "Audi",
                    model: "TT",
                    color: "Black",
                    mileage: 25400,
                    year: 2020,
                    imageUrl: "https://cdn.pixabay.com/photo/2016/09/01/15/06/audi-1636320_960_720.jpg",
                    reservePrice: 20000
                ),

                CreateLiveAuction(
                    id: "3659ac24-29dd-407a-81f5-ecfe6f924b9b",
                    seller: "bob",
                    reservePrice: 25000,
                    daysUntilEnd: 13,
                    make: "Ford",
                    model: "Model T",
                    color: "Rust",
                    mileage: 150150,
                    year: 1938,
                    imageUrl: "https://cdn.pixabay.com/photo/2017/08/02/19/47/vintage-2573090_960_720.jpg"
                )
            };
        }

        private static Auction CreateLiveAuction(
            string id,
            string seller,
            int reservePrice,
            int daysUntilEnd,
            string make,
            string model,
            string color,
            int mileage,
            int year,
            string imageUrl)
        {
            return new Auction
            {
                Id = Guid.Parse(id),
                Status = Status.Live,
                ReversePrice = reservePrice,
                Seller = seller,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(daysUntilEnd),
                Item = CreateItem(make, model, color, mileage, year, imageUrl)
            };
        }

        private static Auction CreateFinishedAuction(
            string id,
            string seller,
            string winner,
            int soldAmount,
            int daysAgoEnded,
            string make,
            string model,
            string color,
            int mileage,
            int year,
            string imageUrl,
            int reservePrice = 0)
        {
            return new Auction
            {
                Id = Guid.Parse(id),
                Status = Status.Finished,
                ReversePrice = reservePrice,
                Seller = seller,
                Winner = winner,
                SoldAmount = soldAmount,
                CurrentHighBid = soldAmount,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(-daysAgoEnded),
                Item = CreateItem(make, model, color, mileage, year, imageUrl)
            };
        }

        private static Auction CreateReserveNotMetAuction(
            string id,
            string seller,
            int reservePrice,
            int currentHighBid,
            int daysAgoEnded,
            string make,
            string model,
            string color,
            int mileage,
            int year,
            string imageUrl)
        {
            return new Auction
            {
                Id = Guid.Parse(id),
                Status = Status.ReservedNotMet,
                ReversePrice = reservePrice,
                Seller = seller,
                CurrentHighBid = currentHighBid,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(-daysAgoEnded),
                Item = CreateItem(make, model, color, mileage, year, imageUrl)
            };
        }

        private static Item CreateItem(
            string make,
            string model,
            string color,
            int mileage,
            int year,
            string imageUrl)
        {
            return new Item
            {
                Make = make,
                Model = model,
                Color = color,
                Mileage = mileage,
                Year = year,
                ImageUrl = imageUrl
            };
        }
    }
}
