from os import getcwd

ROOT_DIR = getcwd()

API_BASE_URL = "http://localhost:9939"

USER_LINK_REGEX = r"^[a-zA-Z0-9_]{6,32}$"
USER_WELCOME_REGEX = r"^.{6,256}$"

