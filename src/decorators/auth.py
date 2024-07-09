import requests

from functools import wraps
from telegram import Update
from telegram.ext import CallbackContext

from src.const import API_BASE_URL


def auth(func):
    """
    Register the User if his not already registered.
    """

    @wraps(func)
    def wrapper(this, update: Update, context: CallbackContext):
        request = requests.get(f"{API_BASE_URL}/api/user", params={
            "telegramId": update.message.from_user.id,
        })

        if request.status_code == 404:
            # Create new User.
            requests.put(
                f"{API_BASE_URL}/api/user/{update.message.from_user.id}",
                headers={
                    "Content-Type": "application/json",
                }
            )

        return func(this, update, context)

    return wrapper
