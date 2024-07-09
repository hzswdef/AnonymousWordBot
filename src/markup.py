from telegram import InlineKeyboardMarkup, InlineKeyboardButton


class Markup(object):

    @staticmethod
    def start_command():
        return InlineKeyboardMarkup([[
            InlineKeyboardButton(text="", callback_data="")
        ]])
