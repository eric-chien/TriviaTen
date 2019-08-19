resource "aws_cognito_user_pool" "user_pool" {
  name = "${var.name}-user-pool"
  tags = "${var.tags}"

  password_policy {
    minimum_length    = 6
    require_lowercase = true
    require_uppercase = true
    require_numbers   = true
  }

  schema {
    name                = "username"
    attribute_data_type = "String"
  }
}

resource "aws_cognito_user_pool_client" "user_pool_client" {
  name = "${var.name}-user-pool-client"

  user_pool_id        = "${aws_cognito_user_pool.user_pool.id}"
  explicit_auth_flows = ["ADMIN_NO_SRP_AUTH"]
}