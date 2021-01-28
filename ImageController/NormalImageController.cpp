#include "pch.h"
/**
 * @file	NormalImageController.cpp
 * @author	kchary6436
 */

#include <memory>
#include "NormalImageController.h"

namespace Kchary::ImageController::NormalImageControl
{
    int NormalImageController::GetImageData(const char* path, ImageData* imageData) const
    {
        // Read image data to mat.
        cv::Mat img = cv::imread(path, cv::ImreadModes::IMREAD_COLOR);
        
        const auto dataSize = img.total() * img.elemSize();
        imageData->buffer = new std::uint8_t[dataSize];
        memcpy(imageData->buffer, img.data, dataSize * sizeof(std::uint8_t));

        // Translate data to C#
        imageData->size = (unsigned int)dataSize;
        imageData->stride = (int)img.step;
        imageData->width = img.cols;
        imageData->height = img.rows;

        img.release();

        return 0;
    }

    int NormalImageController::GetThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData) const
    {
        // Read image data to mat.
        const auto imreadMode = GetImreadMode(resizeLongSideLength);
        cv::Mat img = cv::imread(path, imreadMode);

        // Resize image data.
        const int longSideLength = img.cols > img.rows ? img.cols : img.rows;
        const double ratio = ((double)resizeLongSideLength / (double)longSideLength);
        cv::Mat resizeImg;
        cv::resize(img, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

        const auto dataSize = resizeImg.total() * resizeImg.elemSize();
        imageData->buffer = new std::uint8_t[dataSize];
        memcpy(imageData->buffer, resizeImg.data, dataSize * sizeof(std::uint8_t));

        // Translate data to C#
        imageData->size = (unsigned int)dataSize;
        imageData->stride = (int)resizeImg.step;
        imageData->width = resizeImg.cols;
        imageData->height = resizeImg.rows;

        img.release();
        resizeImg.release();

        return 0;
    }

    cv::ImreadModes NormalImageController::GetImreadMode(int resizeLongSideLength) const
    {
        auto imreadMode = cv::ImreadModes::IMREAD_COLOR;
        if (resizeLongSideLength <= 800)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
        }
        else if (800 < resizeLongSideLength && resizeLongSideLength <= 1600)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
        }
        else if (1600 < resizeLongSideLength && resizeLongSideLength <= 3200)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
        }
        else
        {
            imreadMode = cv::ImreadModes::IMREAD_COLOR;
        }

        return imreadMode;
    }
}