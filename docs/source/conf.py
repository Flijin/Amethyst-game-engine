# Configuration file for the Sphinx documentation builder.
#
# For the full list of built-in configuration values, see the documentation:
# https://www.sphinx-doc.org/en/master/usage/configuration.html

# -- Project information -----------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#project-information

project = 'Amethyst Game Engine'
copyright = '2024, Flijin'
author = 'Flijin'
release = 'v0.1'

# -- General configuration ---------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#general-configuration

extensions = []

templates_path = ['_templates']
exclude_patterns = []
extensions = ['sphinx_wagtail_theme']

language = 'ru'

# -- Options for HTML output -------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#options-for-html-output
html_favicon = '_static/Logo.ico'
html_theme = 'sphinx_wagtail_theme'
html_static_path = ['_static']

html_css_files = [
    'text background.css',
]

html_theme_options = dict(
    project_name = "Amethyst Game Engine",
    logo = "Logo minimalistic.svg",
    logo_alt = "Amethyst",
    logo_height = 60,
    logo_width = 60,
        footer_links = ",".join([
        "GitHub|https://github.com/Flijin/Amethyst-game-engine",
        "Discussions|https://github.com/Flijin/Amethyst-game-engine/discussions",
    ]),
)