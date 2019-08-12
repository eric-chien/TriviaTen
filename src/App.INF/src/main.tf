provider "aws" {
  version                 = "~> 2.0"
  region                  = "${var.aws_region}"
  shared_credentials_file = "/secrets/credentials"
  profile                 = "terraform-user"
}

resource "aws_s3_bucket" "trivia_ten" {  //TEST BUCKET TO TEST BUILD/PLAN/DESTROY SCRIPTS
  bucket = "trivia-ten-test-bucket"
  acl    = "private"
}