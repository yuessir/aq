﻿using System;

namespace AQS.OrderProject.Application.Orders
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; }

        

        public string Name { get; set; }

        public ProductDto()
        {
            
        }

        public ProductDto(Guid id, int quantity)
        {
            this.Id = id;
            this.Quantity = quantity;
        }
    }
}