#############
# Providers #
#############
provider "aws" {
  version                 = "~> 2.0"
  region                  = "${var.aws_region}"
  shared_credentials_file = "/secrets/credentials"
  profile                 = "terraform-user"
}

##########
# Locals #
##########
locals {
  name       = "triviaten"
  prefix     = "${var.environment}-${local.name}"
  ssm_prefix = "/${var.environment}/${local.name}"
  tags = {
    App_Name    = "${local.name}"
    Environment = "${var.environment}"
  }
}

###########
# Modules #
###########
module "cognito" {
  source = "./modules/cognito"
  tags = "${local.tags}"
  name = "${local.prefix}"
}

module "ssm" {
  source = "./modules/ssm"
  tags   = "${local.tags}"
  prefix = "${local.ssm_prefix}"

  ui_app_url = "${var.ui_app_url}"
  cognito_authority_url = "${module.cognito.user_pool_endpoint}"
  cognito_user_pool_id = "${module.cognito.user_pool_id}"
  cognito_client_id = "${module.cognito.user_pool_client_id}"
}