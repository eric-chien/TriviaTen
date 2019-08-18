variable "aws_region" {
  type        = "string"
  description = "Provider variable: Specifies which region to create aws resources in"
}

variable "environment" {
  type        = "string"
  description = "The environment resources will be created for"
}

variable "ui_app_url" {
  type        = "string"
  description = "The url of the client side app"
}
