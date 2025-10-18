@api @products
Feature: Product Registration

Scenario: Create a new product successfully
  Given I provide the data for a new product with name "Test Product" and price 99.99
  When I send a request to create the product
  Then the product should be created successfully
  And I should be able to retrieve this product by its ID

Scenario: Reject product creation with invalid price
  Given I provide the data for a new product with name "Invalid Product" and price -50
  When I send a request to create the product
  Then the request should fail with error "Preco deve ser maior que zero"