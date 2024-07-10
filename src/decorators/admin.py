from functools import wraps

import requests
from telegram import Update
from telegram.ext import CallbackContext

from src.const import API_BASE_URL
from src.helpers.user_roles import UserRoles


def admin(func):
    """
    Register the User if his not already registered.
    """

    @wraps(func)
    def wrapper(this, update: Update, context: CallbackContext):
        user = requests.get(f"{API_BASE_URL}/api/user", params={
            "telegramId": update.effective_user.id,
        })

        if user.status_code != 200:
            return

        user = user.json()

        if UserRoles.Special in UserRoles(user["roles"]):
            return func(this, update, context)

        return None

    return wrapper
