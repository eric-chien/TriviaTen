# resource "aws_ssm_parameter" "cognito_authority_url" {
#     name = "${var.prefix}/cognito-authority-url"
#     type = "SecureString"
#     value = "${var.authority_url}"
#     tags = "${var.tags}"
#     overwrite = true
# }

# resource "aws_ssm_parameter" "cognito_client_id" {
#     name = "${var.prefix}/cognito-client-id"
#     type = "SecureString"
#     value = "${var.client_id}"
#     tags = "${var.tags}"
#     overwrite = true
# }

resource "aws_ssm_parameter" "ui_app_url" {
  name      = "${var.prefix}/ui-app-url"
  type      = "SecureString"
  value     = "${var.ui_app_url}"
  tags      = "${var.tags}"
  overwrite = true
}