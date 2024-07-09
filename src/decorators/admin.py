from functools import wraps
from telegram import Update
from telegram.ext import CallbackContext
from json import load

from src.const import ROOT_DIR


def admin(func):
    """
    Register the User if his not already registered.
    """

    @wraps(func)
    def wrapper(this, update: Update, context: CallbackContext):
        with open(ROOT_DIR + "/.whitelist.json", "r") as file:
            whitelist = load(file)["whitelist"]

        if update.message.from_user.id in whitelist:
            return func(this, update, context)

        return None

    return wrapper
