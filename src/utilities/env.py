from os import environ
from json import load
from dotenv import load_dotenv

from src.const import ROOT_DIR


class Env(object):
    """
    Store environment variables.
    """

    TELEGRAM_BOT_TOKEN = None
    TELEGRAM_ERROR_NOTIFICATIONS_CHANNEL_ID = None
    TELEGRAM_STORAGE_CHANNEL_ID = None

    def __init__(self):
        pass


def load_env() -> Env:
    """
    Load the environment variable to set them into env object.
    """

    load_dotenv(ROOT_DIR + '/.env')

    env_object = Env()

    # Load project settings.
    with open('{path}/settings.json'.format(path=ROOT_DIR), 'r') as settings_file:
        settings = load(settings_file)

    available_variables = settings['secrets']['environment_variables']

    # Set the environment variables to the env object.
    for environment_variable in available_variables:
        setattr(
            env_object,
            environment_variable,
            environ.get(environment_variable),
        )

    return env_object
