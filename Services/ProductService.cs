using App.Models;

namespace App.Services;

public class ProductService : List<ProductModel>{
    public ProductService() {
        this.AddRange(new ProductModel[]{
            new ProductModel() {Id =1, Name ="InPhone", Price=100},
            new ProductModel() {Id =2, Name ="InPhone1", Price=400},
            new ProductModel() {Id =3, Name ="InPhone2", Price=200},
            new ProductModel() {Id =4, Name ="InPhone3", Price=300},
        });
    }
}