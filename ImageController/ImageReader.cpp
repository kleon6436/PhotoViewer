/*!
 * @file	ImageReader.cpp
 * @author	kleon6436
 *
 */

#include "pch.h"
#include "RawImageController.h"
#include "NormalImageController.h"
#include "ImageReader.h"
#include <locale.h>
#include <iostream>

namespace Kchary::ImageController::Library
{
	using namespace Kchary::ImageController::RawImageControl;
	using namespace Kchary::ImageController::NormalImageControl;

	ImageReader::ImageReader()
	{
		m_rawImageController = std::make_unique<RawImageController>();
		m_normalImageController = std::make_unique<NormalImageController>();
	}

	bool ImageReader::GetImageData(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, ImageData& imageData)
	{
		bool result = false;

		if (imageReadSettings.isRawImage)
		{
			result = m_rawImageController->GetImageData(imagePath, imageReadSettings, imageData);
		}
		else
		{
			result = m_normalImageController->GetImageData(imagePath, imageReadSettings, imageData);
		}

		return result;
	}
}