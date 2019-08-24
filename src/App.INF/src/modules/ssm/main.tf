resource "aws_ssm_parameter" "cognito_authority_url" {
    name = "${var.prefix}/cognito-authority-url"
    type = "SecureString"
    value = "https://${var.cognito_authority_url}"
    tags = "${var.tags}"
    overwrite = true
}

resource "aws_ssm_parameter" "cognito_user_pool_id" {
    name = "${var.prefix}/cognito-user-pool-id"
    type = "SecureString"
    value = "${var.cognito_user_pool_id}"
    tags = "${var.tags}"
    overwrite = true
}

resource "aws_ssm_parameter" "cognito_client_id" {
    name = "${var.prefix}/cognito-client-id"
    type = "SecureString"
    value = "${var.cognito_client_id}"
    tags = "${var.tags}"
    overwrite = true
}

resource "aws_ssm_parameter" "ui_app_url" {
  name      = "${var.prefix}/ui-app-url"
  type      = "SecureString"
  value     = "${var.ui_app_url}"
  tags      = "${var.tags}"
  overwrite = true
}